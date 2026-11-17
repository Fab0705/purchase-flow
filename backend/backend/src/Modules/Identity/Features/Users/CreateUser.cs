using backend.Api.Modules.Identity.Domain.Entities;
using backend.Common.Contracts;
using backend.Common.Interfaces;
using backend.Modules.Identity.Database;
using backend.Modules.Identity.Features._EndpointGroup;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Identity.Features.Users
{
    public static class CreateUser
    {
        public record Command(string Email, string Role, Guid? LocationId) : IRequest<UserResponse>;
        public record UserResponse(Guid Id, string Email, string TemporaryPassword, string Message);

        public class Handler : IRequestHandler<Command, UserResponse>
        {
            private readonly ISender _sender;
            private readonly IdentityDbContext _dbContext;
            private readonly IPasswordHasher _passwordHasher;

            public Handler(IdentityDbContext dbContext, IPasswordHasher passwordHasher, ISender sender)
            {
                _dbContext = dbContext;
                _passwordHasher = passwordHasher;
                _sender = sender;
            }

            public async Task<UserResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                if (request.LocationId.HasValue && request.LocationId.Value != Guid.Empty)
                {
                    bool locationExists = await _sender.Send(new CheckLocationExistsQuery(request.LocationId.Value), cancellationToken);
                    if (!locationExists)
                    {
                        throw new ValidationException("La ubicación asignada no existe en el catálogo global de inventario.");
                    }
                }

                var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
                if (existingUser != null)
                {
                    throw new Exception("User with this email already exists.");
                }

                var temporaryPassword = _passwordHasher.GenerateTemporaryPassword();
                var passwordHash = _passwordHasher.HashPassword(temporaryPassword);

                var newUser = new User(request.Email, passwordHash, request.Role, request.LocationId);

                // Add the new user to the database
                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new UserResponse(newUser.Id, newUser.Email, temporaryPassword, "Usuario creado correctamente, se ha enviado un correo electrónico con la contraseña temporal.");
            }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Role).NotEmpty();
                RuleFor(x => x.LocationId)
                    .NotEmpty().WithMessage("La ubicación es obligatoria para los roles operativos.")
                    .NotEqual(Guid.Empty).WithMessage("Debe seleccionar una ubicación válida.")
                    .When(x => !string.Equals(x.Role, "Administrator", StringComparison.OrdinalIgnoreCase)
                                && !string.Equals(x.Role, "ConsultantLogistic", StringComparison.OrdinalIgnoreCase));
            }
        }

        public class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapIdentityGroup().MapPost("/create-user", async (Command command, IMediator mediator) =>
                    {
                        var result = await mediator.Send(command);
                        return Results.Ok(result);
                    })
                    .WithName("CreateUser")
                    .AllowAnonymous();
            }
        }
    }
}