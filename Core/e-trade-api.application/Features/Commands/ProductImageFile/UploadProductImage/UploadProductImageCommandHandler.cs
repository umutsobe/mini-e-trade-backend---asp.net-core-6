using e_trade_api.application;
using e_trade_api.domain;
using e_trade_api.domain.Entities;
using MediatR;

public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommandRequest, UploadProductImageCommandResponse>
{

    readonly IStorageService _storageService;

    readonly IProductReadRepository _productReadRepository;
    readonly IProductImageFileWriteRepository _productImageFileWriteRepository;

    public UploadProductImageCommandHandler(IStorageService storageService, IProductReadRepository productReadRepository, IProductImageFileWriteRepository productImageFileWriteRepository)
    {
        _storageService = storageService;
        _productReadRepository = productReadRepository;
        _productImageFileWriteRepository = productImageFileWriteRepository;
    }
    public async Task<UploadProductImageCommandResponse> Handle(UploadProductImageCommandRequest request, CancellationToken cancellationToken)
    {
        List<(string fileName, string path)> datas = await _storageService.UploadAsync(
            "product-image",
            request.Files
        );

        Product product = await _productReadRepository.GetByIdAsync(request.Id);

        await _productImageFileWriteRepository.AddRangeAsync(
            datas
                .Select(
                    d =>
                        new ProductImageFile()
                        {
                            FileName = d.fileName,
                            Path = d.path,
                            Storage = _storageService.StorageName,
                            Products = new List<Product>() { product }
                        }
                )
                .ToList()
        );
        await _productImageFileWriteRepository.SaveAsync();
        return new();
    }
}

// string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "resource/product-images");: Dosyaların yükleneceği dizinin tam yolu olan uploadPath değişkenini oluşturur. Path.Combine metodu, WebRootPath (wwwroot klasörü) ve "resource/product-images" klasör yolunu birleştirerek dosyaların yükleneceği dizini elde eder.

// foreach (var file in Request.Form.Files): HTTP isteği ile gönderilen dosyaları döngü ile işler. Request.Form.Files, gönderilen dosya verilerine erişimi sağlayan bir koleksiyondur.

// Dosya ile ilgili işlemler yapmak için file değişkenini kullanarak döngü içerisinde devam edilir.

// Dosyanın kaydedileceği tam yol olan fullPath oluşturulur. Path.GetExtension(file.FileName) ile dosya uzantısı alınır ve dosya adının önüne rastgele bir sayı eklenerek dosyanın benzersiz bir isim alması sağlanır.

// using FileStream fileStream = new(...): Dosyanın içeriğini sunucuda belirtilen yola kaydetmek için bir FileStream örneği oluşturulur. using bloğu, fileStream nesnesinin işi bittiğinde otomatik olarak kapatılmasını sağlar.

// Dosyayı sunucuda belirtilen yola FileMode.Create ile oluşturulacak şekilde açar. FileAccess.Write ile dosyaya yazılabilir bir erişim hakkı verilir.

// await file.CopyToAsync(fileStream);: Dosya içeriği, HTTP isteği ile gelen dosya verileriyle fileStream nesnesine asenkron olarak kopyalanır. Dosya yüklemesi için asenkron yöntem kullanılmasının nedeni, sunucunun dosya yüklemelerini eş zamanlı olarak işlemesine olanak sağlamaktır.

// await fileStream.FlushAsync();: Dosyaya yazma işlemi tamamlandığında, fileStream örneği kullanılarak verilerin diske tamamen yazıldığından emin olmak için FlushAsync metodu çağrılır.

// Döngü, diğer dosyaları yüklemek için devam eder.

// return Ok();: Eylem, dosyaların başarıyla yüklendiğini belirten bir Ok cevabı döndürür.