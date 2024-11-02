using Common.Contracts;
using FinanceService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

// public class FinanceCreateConsumer : IConsumer<FinanceAddCredit>
// {
//     private readonly FinanceDbContext _dbContext;
//     private readonly IPublishEndpoint _publishEndpoint;

//     public FinanceCreateConsumer(FinanceDbContext dbContext, IPublishEndpoint publishEndpoint)
//     {
//         _dbContext = dbContext;
//         _publishEndpoint = publishEndpoint;
//     }
//     public async Task Consume(ConsumeContext<FinanceAddCredit> context)
//     {
//         using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
//         var correlationId = context.Message.FinanceActionsList[0].CorrelationId;
//         foreach (var item in context.Message.FinanceActionsList)
//         {
//             switch (item.OperationType)
//             {
//                 case OperationType.Insert:
//                     //добавляем новое поступление денег на счет
//                     await _dbContext.FinanceItems.AddAsync(item.FinanceItem);
//                     break;
//                 case OperationType.Update:
//                     //обновляем запись текущего баланса
//                     var finItem = await _dbContext.FinanceItems.Where(p => p.UserLogin == item.FinanceItem.UserLogin &&
//                     p.Status == FinanceRecordStatus.Баланс).FirstOrDefaultAsync();
//                     if (finItem == null)
//                     {
//                         var newBalance = new FinanceItem();
//                         newBalance.ActionDate = item.FinanceItem.ActionDate;
//                         newBalance.Id = item.FinanceItem.Id;
//                         newBalance.Status = item.FinanceItem.Status;
//                         newBalance.UserLogin = item.FinanceItem.UserLogin;
//                         newBalance.Value = item.FinanceItem.Value;
//                         await _dbContext.FinanceItems.AddAsync(newBalance);
//                     }
//                     else
//                     {
//                         finItem.Value = item.FinanceItem.Value;
//                     }
//                     break;
//             }
//         }
//         await _dbContext.SaveChangesAsync();
//         await transaction.CommitAsync();
//         await _publishEndpoint.Publish(new FinanceCreated(correlationId));
//     }
// }
