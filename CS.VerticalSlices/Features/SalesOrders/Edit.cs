using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CS.VerticalSlices.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using FluentValidation;
using System.Collections.Generic;
using CS.VerticalSlices.Data.Entities;

namespace CS.VerticalSlices.Features.SalesOrders
{
    public class Edit
    {
        public class Command : IRequest<string>
        {
            public string Number { get; set; }

            public List<Line> Lines { get; set; }

            public class Line
            {
                public int Number { get; set; }
                public string ProductCode { get; set; }
                public int Quantity { get; set; }
            }

            public void Validate()
            {
                var validator = new Validator();
                var validationResult = validator.Validate(this);
                if (!validationResult.IsValid) throw new Exception(validationResult.ToString());
            }

            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.Number).NotEmpty();
                }
            }
        }

        public class Handler : IRequestHandler<Command, string>
        {
            private readonly ShopContext context;

            public Handler(ShopContext context) => this.context = context;

            public async Task<string> Handle(Command request, CancellationToken cancellationToken)
            {
                request.Validate();

                var model = 
                    await this.context.SalesOrders
                    .SingleOrDefaultAsync(order => order.Number == request.Number);

                model.Lines = 
                    request.Lines
                    .Select(line => new SalesOrder.Line 
                    { 
                        Number = line.Number, 
                        Product = this.context.Products.SingleOrDefault(product => product.Code == line.ProductCode), 
                        Quantity = line.Quantity 
                    }).ToList();

                await this.context.SaveChangesAsync();

                return request.Number;
            }
        }
    }
}
