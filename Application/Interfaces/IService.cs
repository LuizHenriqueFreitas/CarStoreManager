//Interface Generica que faz o crud basico de todas as entidades
using CarStoreManager.Application.Common;

namespace CarStoreManager.Application.Interfaces;

public interface IService<
    TDTO, 
    TListDTO, 
    TAddDTO, 
    TUpdateDTO>
{
    //GET/id
    Task<Result<TDTO>> GetByIdAsync(Guid id);
    //GET
    Task<Result<IEnumerable<TListDTO>>> GetAllAsync();
    //POST
    Task<Result<Guid>> AddAsync(TAddDTO entity);
    //PUT
    Task<Result> UpdateAsync(TUpdateDTO entity);
    //DELETE
    Task<Result> RemoveAsync(Guid id);
 }