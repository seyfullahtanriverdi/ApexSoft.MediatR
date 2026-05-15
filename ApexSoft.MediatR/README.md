# ApexSoft.MediatR

ApexSoft.MediatR is a lightweight and high-performance Mediator library that enables you to implement the 
CQRS (Command Query Responsibility Segregation) pattern in your .NET projects.
This library was developed as an alternative to the MediatR library, specifically optimized for a wide range of projects, 
including modern versions like .NET 8.0 and .NET 9.0.

Features
1. Lightweight and Fast: A pure CQRS structure stripped of unnecessary dependencies.
2. Multi-Targeting: Fully compatible with .NET 8.0 and 9.0 versions.
3. Easy Integration: Offers single-line registration via IServiceCollection.
4. Modern C# Support: Supports Primary Constructors and modern syntax features.

Installation
You can automatically register all Handler structures by adding the following definition to the part of 
your project where dependencies are managed (e.g., the ApplicationRegistration class):

Registering — Program.cs

    using ApexSoft.MediatR;

    var builder = WebApplication.CreateBuilder(args);

    // Tüm handler'ları otomatik tarar ve register eder
    builder.Services.AddMediator(
        handlerLifetime: ServiceLifetime.Scoped,
        assemblies: typeof(Program).Assembly
    );

If you want to scan multiple assemblies:

    csharpbuilder.Services.AddMediator(
        handlerLifetime: ServiceLifetime.Scoped,
        assemblies: typeof(Program).Assembly,
                  typeof(SomeOtherClass).Assembly
    );

Usage Examples
Query — Returning a Response

    // GetUserQuery.cs
    public record GetUserQuery(int Id) : IRequest<UserDto>;

    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
    {
        private readonly AppDbContext _db;
        public GetUserQueryHandler(AppDbContext db) => _db = db;

        public async Task<UserDto> Handle(GetUserQuery request, CancellationToken ct)
        {
            var user = await _db.Users.FindAsync(request.Id, ct);
            return new UserDto(user.Id, user.Name);
        }
    }

Command — Void (Unit)

    // GetUserQuery.cs
    public record CreateUserCommand(string Name, string Email) : IRequest;

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
    {
        private readonly AppDbContext _db;
        public CreateUserCommandHandler(AppDbContext db) => _db = db;

        public async Task<Unit> Handle(CreateUserCommand request, CancellationToken ct)
        {
            _db.Users.Add(new User { Name = request.Name, Email = request.Email });
            await _db.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }

Notification — Publishing Events

    //UserCreatedEvent.cs
    public record UserCreatedEvent(string Name, string Email) : INotification;

    // İlk handler
    public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedEvent>
    {
        public async Task Handle(UserCreatedEvent notification, CancellationToken ct)
        {
            // email gönder...
        }
    }

    // İkinci handler (aynı event'e birden fazla handler olabilir)
    public class LogUserCreatedHandler : INotificationHandler<UserCreatedEvent>
    {
        public async Task Handle(UserCreatedEvent notification, CancellationToken ct)
        {
            // log yaz...
        }
    }

Pipeline Behavior — Cross-cutting Concerns
    
    // LoggingBehavior.cs
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

        public async Task<TResponse> Handle(TRequest request, CancellationToken ct, Func<Task<TResponse>> next)
        {
            _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
            var response = await next();
            _logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
            return response;
        }
    }

Usage in Controllers or Minimal APIs
Minimal API

    csharpapp.MapGet("/users/{id}", async (int id, ISender sender, CancellationToken ct) =>
    {
        var result = await sender.Send(new GetUserQuery(id), ct);
        return Results.Ok(result);
    });

    app.MapPost("/users", async (CreateUserCommand command, ISender sender, CancellationToken ct) =>
    {
        await sender.Send(command, ct);
        return Results.Created();
    });

Controller

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(ISender sender) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken ct)
        {
            var result = await sender.Send(new GetUserQuery(id), ct);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserCommand command, CancellationToken ct)
        {
            await sender.Send(command, ct);
            return Created();
        }

        [HttpPost("{id}/notify")]
        public async Task<IActionResult> Notify(int id, IMediator mediator, CancellationToken ct)
        {
            await mediator.Publish(new UserCreatedEvent("Ad", "email@test.com"), ct);
            return Ok();
        }
    }

Architecture
This library allows you to manage commands and queries independently, in accordance with Onion Architecture or Clean Architecture principles.

Developer: Seyfullah Tanrıverdi