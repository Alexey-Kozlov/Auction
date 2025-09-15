using System.Security.Claims;
using Common.Contracts;
using Common.Contracts.Finance;
using FinanceService.DTO;
using FinanceService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ImageService.Controllers;


[Authorize]
[ApiController]
[Route("api/finance")]
public class FinanceController : ControllerBase
{
    private readonly GetFinanceService _getFinanceService;


    public FinanceController(GetFinanceService getFinanceService)
    {
        _getFinanceService = getFinanceService;
    }

    [HttpGet("GetBalance")]
    public async Task<ApiResponse<int>> GetBalance()
    {
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login")
            .Select(p => p.Value).FirstOrDefault();

        return await _getFinanceService.GetBalance(userLogin);
    }

    /*
    Получаем записи финансовых операций по данному пользователю.
    На входе - пусто, без параметров сортировки и пагинации - PrimeReact не поддерживает пагинацию
    и сортировку на сервере, все выполняется на клиенте.
    На выходе - запрошенная страница с данными.
    Особенности.
    Нужно получить структуру данных с полями из 2-х БД - БД финансов и БД Аукционов.
    Из БД Финансов получаем дату операции, пользователя, сумму, тип операции (приход.расход).
    Из БД Аукциона получаем наименование аукциона (если расход) и создателя аукциона.
    Что делаем - выбираем ВСЕ записи для данного пользователя из БД Финансов и дополняем эти 
    записи полями с данными из БД Аукциона (Seller и Title).
    Из БД Финансов выбираем ВСЕ записи пользователя, без пагинации - т.к. только потом, после объединения
    с данными БД Аукциона - мы сможем отсортировать по нужному полю и взять нужную страницу.
    Решение не эффективное, но другого с рапределенными БД быть не может.
    Работает все синхронно, через GRPC. 
    Почему - на клиенте контрол DateTable - не умеет асинхронно сортировать данные по столбцам, если на
    клиенте использовать для этого функционала useState и прочие фичи - получаем бесконечный цикл
    перезагрузки страницы при сортировке. Единственное решение - использовать синхронный вызов WebApi
    и ждать возврата результата. Использование GRPC делает работу функционала по передаче большого объема
    данных максимально эффективным.
    В предыдущей версии финансового блока, где клиент был на Semantic UI - все работало асинхронно,
    через сообщения, была пагинация и сортировка на сервере, там таблица с данными на клиенте 
    поддерживала асинхронную работу.
    */
    [HttpGet("GetHistory")]
    public async Task<ApiResponse<PagedResult<List<FinanceHistoryItem>>>> GetHistory([FromQuery] PagedParamsDTO pagedParamsDTO)
    {
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login")
            .Select(p => p.Value).FirstOrDefault();

        return await _getFinanceService.GetHistory(pagedParamsDTO, userLogin);
    }

}
