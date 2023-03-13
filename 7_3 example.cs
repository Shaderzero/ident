//пример реализации паттерна репозитория

public class CountryRepository: ICountryRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CountryRepository(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }
    
    public async Task<CountryDto> CreateAsync(CountryDto dto)
    {
        var toDb = _mapper.Map<CountryDto, Country>(dto);
        var result = await _db.Countries.AddAsync(toDb);
        await _db.SaveChangesAsync();
        return _mapper.Map<Country, CountryDto>(result.Entity);
    }

    public async Task<bool> UpdateAsync(CountryDto dto)
    {
        var fromDb = await _db.Countries.FindAsync(dto.Id);
        if (fromDb == null) return false;
        _mapper.Map(dto, fromDb);
        _db.Countries.Update(fromDb);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<CountryDto> GetAsync(int id)
    {
        var result = await _db.Countries
            .Where(x => x.Id == id)
            .ProjectTo<CountryDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<int> DeleteAsync(int id)
    {
        var fromDb = await _db.Countries
            .Include(x => x.Names)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
        if (fromDb == null) return 0;
        {
            var parent = await _db.DirectionCountries
                .Where(x => x.CountryId == id)
                .AnyAsync();
            if (parent)
            {
                return -1;
            }
            _db.Countries.Remove(fromDb);
            return await _db.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<CountryDto>> GetAll()
    {
        var result = await _db.Countries
            .ProjectTo<CountryDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
        // var dtos = _mapper.Map<IEnumerable<Country>, IEnumerable<CountryDto>>(entities);
        return result;
    }

    public async Task<CountryDto> IsUnique(CountryDto dto)
    {
        if (dto.Id == 0)
        {
            var result = await _db.Countries
                .ProjectTo<CountryDto>(_mapper.ConfigurationProvider)
                .Where(x => string.Equals(x.Name, dto.Name) || x.Names.Contains(dto.Name))
                .FirstOrDefaultAsync();
            return result;
        }
        else
        {
            var result = await _db.Countries
                .Where(x => (string.Equals(x.Name, dto.Name) || x.Names.Contains(dto.Name))
                            && x.Id != dto.Id)
                .ProjectTo<CountryDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            return result;
        }
    }

    public async Task<PagedList<CountryDto>> GetPaged(PaginationParams parameters)
    {
        var source = _db.Countries
            .AsQueryable();
        source = source.Search(parameters.Filter);
        source = source.Sort(parameters.Order, parameters.OrderAsc);
        var result = await PagedList<Country>.CreateAsync(source, parameters.PageNumber, parameters.PageSize);
        var entities = _mapper.Map<List<CountryDto>>(result);

        return new PagedList<CountryDto>(entities, entities.Count, parameters.PageNumber, parameters.PageSize);
    }
}

public static class CountryRepositoryExtensions
{
    public static IQueryable<Country> Search(this IQueryable<Country> source, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return source;
        var f = filter.ToLower();
        var result = source.Where(s => s.Name.ToLower().Contains(f)
                                       || s.ShortName.ToLower().Contains(f));
        return result;
    }

    public static IQueryable<Country> Sort(this IQueryable<Country> source, string columnName, bool ascending)
    {
        if (string.IsNullOrWhiteSpace(columnName))
        {
            source = source.OrderBy(s => s.Name);
        }
        else
        {
            var prop = TypeDescriptor.GetProperties(typeof(Country)).Find(columnName, true);
            source = ascending ? source.OrderBy(x => prop.GetValue(x)) : source.OrderByDescending(x => prop.GetValue(x));
        }
        return source;
    }

    public static IQueryable<Country> Sort(this IQueryable<Country> source, string sorter)
    {
        if (string.IsNullOrWhiteSpace(sorter))
        {
            source = source.OrderBy(s => s.Name);
        }
        else
        {
            var columnName = sorter.Split(' ')[0];
            var sortAsc = sorter.Split(' ')[1] == "asc" ? true : false;
            var prop = TypeDescriptor.GetProperties(typeof(Country)).Find(columnName, true);
            source = sortAsc ? source.OrderBy(x => prop.GetValue(x)) : source.OrderByDescending(x => prop.GetValue(x));
                
        }
        return source;
    }
}