using System.Threading;
using System.Threading.Tasks;
using CS.VerticalSlices.Data;
using MediatR;
using FluentValidation;
using System.Collections.Generic;
using System;
using System.Linq;

namespace CS.VerticalSlices.Features.SalesOrders
{
    public class Create
    {
        public class Command : IRequest<string>
        {
            public List<OrderLine> Lines { get; set; }

            public DateTime DateCreated { get; set; }

            public void Validate()
            {
                var validator = new Validator();
                var validationResult = validator.Validate(this);
                if (!validationResult.IsValid) throw new Exception(validationResult.ToString());
            }

            public class OrderLine
            {
                public string ProductCode { get; set; }

                public int Quantity { get; set; }
            }

            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(order => order.DateCreated).NotEmpty();
                    RuleFor(order => order.Lines)
                        .NotEmpty()
                        .ForEach(ruleBuilder => ruleBuilder.ChildRules(validator =>
                        {
                            validator.RuleFor(line => line.ProductCode).NotEmpty();
                            validator.RuleFor(line => line.Quantity).NotEmpty();
                        }));
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

                var orderCount = this.context.SalesOrders.Count();
                var orderNumber = $"ORDER-{request.DateCreated:yy}{orderCount + 1:D6}";
                var order = new Data.Entities.SalesOrder
                {
                    Number = orderNumber,
                    CreatedDate = request.DateCreated,
                    Lines = request.Lines.Select((line, index) => new Data.Entities.SalesOrder.Line
                    {
                        Number = index + 1,
                        Product = this.context.Products.Single(product => product.Code == line.ProductCode),
                        Quantity = line.Quantity
                        // Price = line.Price
                    }).ToList()
                };

                this.context.SalesOrders.Add(order);

                await this.context.SaveChangesAsync();

                return orderNumber;
            }
        }
    }
}
