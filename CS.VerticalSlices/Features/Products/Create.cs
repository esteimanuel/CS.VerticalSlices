using System.Threading;
using System.Threading.Tasks;
using CS.VerticalSlices.Data;
using MediatR;
using FluentValidation;
using System;

namespace CS.VerticalSlices.Features.Products
{
    public class Create
    {
        public class Command : IRequest<string>
        {
            public string Code { get; set; }
            public string Description { get; set; }

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
                    RuleFor(order => order.Code).NotEmpty();
                    RuleFor(order => order.Description).NotEmpty();
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

                var product = new Data.Entities.Product
                {
                    Code = request.Code,
                    Description = request.Description
                };

                this.context.Products.Add(product);

                await this.context.SaveChangesAsync();

                return product.Code;
            }
        }
    }
}
