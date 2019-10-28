using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CS.VerticalSlices.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using FluentValidation;
using System.Collections.Generic;

namespace CS.VerticalSlices.Features.SalesOrders
{
    public class Details
    {
        public class Query : IRequest<Model>
        {
            public string Number { get; set; }

            public void Validate()
            {
                var validator = new Validator();
                var validationResult = validator.Validate(this);
                if (!validationResult.IsValid) throw new Exception(validationResult.ToString());
            }

            public class Validator : AbstractValidator<Query>
            {
                public Validator()
                {
                    RuleFor(x => x.Number).NotEmpty();
                }
            }
        }

        public class Model
        {
            public string Number { get; set; }

            public DateTime CreatedDate { get; set; }

            public List<Line> Lines { get; set; }

            public class Line
            {
                public int Number { get; set; }
                public string ProductCode { get; set; }
                public int Quantity { get; set; }
                public string ProductDescription { get; internal set; }
            }
        }

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly ShopContext context;

            public Handler(ShopContext context) => this.context = context;

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                request.Validate();

                var model =
                    await this.context.SalesOrders
                    .Where(cart => cart.Number == request.Number)
                    .Select(order => new Model 
                    {
                        Number = order.Number,
                        CreatedDate = order.CreatedDate,
                        Lines = order.Lines.Select(line => new Model.Line
                        {
                            Number = line.Number,
                            ProductCode = line.Product.Code,
                            ProductDescription = line.Product.Description,
                            Quantity = line.Quantity
                        }).ToList()
                    })
                    .SingleOrDefaultAsync();

                return model;
            }
        }
    }
}
