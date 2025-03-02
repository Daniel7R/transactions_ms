using PaymentsMS.Application.Interfaces;
using PaymentsMS.Domain.Entities;
using PaymentsMS.Infrastructure.Repository;

namespace PaymentsMS.Application.Services
{
    public class ComissionService : IComissionService
    {
        private const double PERCENT_COMISSION = 0.10;
        private readonly ICreateRepository<Comissions> _comissionsCreateRepository;

        public ComissionService(ICreateRepository<Comissions> comissionsCreateRepository)
        {
            _comissionsCreateRepository = comissionsCreateRepository;
        }

        public async Task TakeComission(int idTransaction, decimal totalTransaction)
        {
            var comissionTotalValue = (double)totalTransaction * PERCENT_COMISSION;
            Comissions comissions = new Comissions
            {
                Percent = (decimal)PERCENT_COMISSION*100,
                Total = (decimal)comissionTotalValue,
                IdTransaction = idTransaction
            };

            await _comissionsCreateRepository.CreateAsync(comissions);
        }
    }
}
