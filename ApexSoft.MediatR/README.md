# ApexSoft.MediatR
ApexSoft.MediatR, .NET projelerinizde CQRS (Command Query Responsibility Segregation) desenini uygulamanızı sağlayan, hafif ve yüksek performanslı bir Mediator kütüphanesidir. Bu kütüphane, özellikle .NET Framework 4.8 gibi legacy sürümlerden .NET 9.0 gibi modern sürümlere kadar geniş bir yelpazede çalışan projeler için MediatR kütüphanesine alternatif olarak geliştirilmiştir.

🚀 Özellikler
Hafif ve Hızlı: Gereksiz bağımlılıklardan arındırılmış saf CQRS yapısı.

Multi-Targeting: .NET Framework 4.8, 6.0, 7.0, 8.0 ve 9.0 sürümleriyle tam uyumludur.

Kolay Entegrasyon: IServiceCollection üzerinden tek satırla kayıt imkanı sunar.

Modern C# Desteği: Primary Constructors ve modern syntax özelliklerini destekler.

🛠 Kurulum
Projenizde bağımlılıkların yönetildiği kısımda (örneğin ApplicationRegistration sınıfı) aşağıdaki tanımlamayı yaparak tüm Handler yapılarını otomatik olarak kaydedebilirsiniz:

C#
public static class ApplicationRegistration
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Belirtilen Assembly içindeki tüm IRequestHandler yapılarını tarar ve kaydeder.
        services.AddMediator(typeof(ApplicationRegistration).Assembly);
    }
}

📖 Örnek Kullanım
1. Query ve Handler Tanımlama
İş mantığınızı Query ve Handler olarak tek bir dosyada organize ederek kod okunabilirliğini artırabilirsiniz:

C#
public class GetAllDriverQuery : IRequest<ServiceResponse<IEnumerable<DriverDto>>>
{
    public QueryOptions QueryOptions { get; set; }

    public class GetAllDriverQueryHandler : IRequestHandler<GetAllDriverQuery, ServiceResponse<IEnumerable<DriverDto>>>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IMapper _mapper;

        public GetAllDriverQueryHandler(IDriverRepository driverRepository, IMapper mapper)
        {
            _driverRepository = driverRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<IEnumerable<DriverDto>>> Handle(GetAllDriverQuery request, CancellationToken cancellationToken)
        {
            var driverEntities = await _driverRepository.GetAllAsync(request.QueryOptions);
            var driverDtos = _mapper.Map<List<DriverDto>>(driverEntities);
            
            return new ServiceResponse<IEnumerable<DriverDto>>(driverDtos, 200, "Success");
        }
    }
}

2. Controller İçinde Kullanım
Handler'ı tetiklemek için ISender arayüzünü enjekte etmeniz yeterlidir:

C#
public class DriverController : ControllerBase
{
    private readonly ISender _sender;

    public DriverController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllDriversAsync(QueryOptions options)
    {
        var query = new GetAllDriverQuery() { QueryOptions = options };
        var result = await _sender.Send(query);
        
        return Ok(result);
    }
}

🏗 Mimari Yakıt
Bu kütüphane; Onion Architecture veya Clean Architecture prensiplerine uygun olarak, komut ve sorgu işlemlerini birbirinden ayırarak yönetmenize olanak tanır.

Geliştirici: Seyfullah Tanrıverdi