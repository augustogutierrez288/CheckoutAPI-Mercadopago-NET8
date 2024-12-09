using System.Diagnostics;
using System.Reflection;
using CheckoutAPI_Mercadopago.Models;
using MercadoPago.Client.Common;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using Microsoft.AspNetCore.Mvc;

namespace CheckoutAPI_Mercadopago.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(SendPaymentDTO model)
        {
            if(model.IdentificationType is null)
            {
                model.IdentificationType = "CC";
            }

            var accessToken = ObtenerAccessToken();
            var publicKey = ObtenerPublicKey();

            //*Mercado pago configuracion
            MercadoPagoConfig.AccessToken = accessToken;
            var productDetails = new Products()
            {
                Id = 1,
                Name = "Xioami C15",
                Unit = 3,
                UnitPrice = 30000,
                CategoryId = 5,
                Description = "The new era of the intelligence",
            };
            //*Crear request hacia mercado pago
            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        //*Esta informacion se trae desde la base de datos/formulario/ carrito de compra
                        Id = productDetails.Id.ToString(),
                        Title = productDetails.Name,
                        CurrencyId = "ARS",
                        PictureUrl = "https://www.mercadopago.com/org-img/MP3/home/logomp3.gif",
                        Description = productDetails.Description,
                        CategoryId = productDetails.CategoryId.ToString(),
                        Quantity = productDetails.Unit,
                        UnitPrice = Convert.ToDecimal(productDetails.UnitPrice)
                    }
                },

                //*Aqui estara la informacion del usuario
                Payer = new PreferencePayerRequest
                {
                    Name = model.Email,
                    Surname = model.Email.ToUpper(),
                    Email = model.Email,
                    Identification = new IdentificationRequest
                    {
                        Type = model.IdentificationType,
                        Number = model.IdentificationNumber
                    },
                    Address = new AddressRequest
                    {
                        StreetName = model.StreetName,
                        StreetNumber = model.StreetNumber,
                        ZipCode = model.ZipCode
                    },
                    Phone = new PhoneRequest
                    {
                        AreaCode = model.PhoneAreaCode,
                        Number = model.PhoneNumber
                    }
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "https://localhost:7207/Success",
                    Failure = "https://localhost:7207/Failure",
                },
                AutoReturn = "approved",
                PaymentMethods = new PreferencePaymentMethodsRequest
                {
                    ExcludedPaymentMethods = [],
                    ExcludedPaymentTypes = [],
                    Installments = 24
                },
                StatementDescriptor = "Sistema de Ventas .net 8",
                ExternalReference = $"Referencia_{Guid.NewGuid().ToString()}",
                Expires = true,
                ExpirationDateFrom = DateTime.Now,
                ExpirationDateTo = DateTime.Now.AddMinutes(10)
            };
            var client = new PreferenceClient();
            Preference preference = await client.CreateAsync(request);
            return Redirect(preference.SandboxInitPoint); // para prueba
            //return Redirect(preference.InitPoint); Para produccion
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("Success")]
        public async Task<IActionResult> Success([FromQuery] PaymentResponse paymentResponse)
        {
            return Json(paymentResponse);
        }
        [HttpGet("Failure")]
        public async Task<IActionResult> Failure([FromQuery] PaymentResponse paymentResponse)
        {
            return Json(paymentResponse);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #region Datos de configuracion para mercadopago

        private string ObtenerAccessToken()
        {
            var token = _configuration.GetValue<string>("MercadoPago:AccessToken");
            return token ?? string.Empty;
        }

        private string ObtenerPublicKey()
        {
            var token = _configuration.GetValue<string>("MercadoPago:PublicKey");
            return token ?? string.Empty;
        }
        #endregion
    }
}
