using System.Threading.Tasks;
using CS.VerticalSlices.Features.SalesOrders;
using Microsoft.AspNetCore.Mvc;

namespace CS.VerticalSlices.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        [HttpGet]
        public async Task<Overview.Page> Get(Overview.Query request, [FromServices] Overview.Handler handler) => 
            await handler.Handle(request, default);

        [HttpGet("{number}", Name = "Get")]
        public async Task<Details.Model> Get(string number, [FromServices] Details.Handler handler) => 
            await handler.Handle(new Details.Query { Number = number }, default);

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Create.Command request, [FromServices] Create.Handler handler) => 
            Created("orders", await handler.Handle(request, default));

        [HttpPut("{number}")]
        public async void Put(string number, [FromBody] Edit.Command request, [FromServices] Edit.Handler handler) 
        {
            request.Number = number;
            await handler.Handle(request, default);
        } 

        [HttpDelete("{number}")]
        public async void Delete(string number, [FromServices] Delete.Handler handler) => 
            await handler.Handle(new Delete.Query { Number = number }, default);
    }
}
