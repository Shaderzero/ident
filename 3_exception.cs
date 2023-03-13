public IQueryable<T> GetAll()
{
    try
   	{
        return dbSet.AsQueryable();
   	}
   	catch (Exception ex)
   	{
         	throw new Exception(ex.Message);
   	}
}

// AsQueryable() => не предполагает выполнения непосредственно запроса на этапе return, возвращается подготовленный запрос для дальнейшей обработки
// параметр T