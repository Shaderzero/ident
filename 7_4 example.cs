//получение большого объема данных для отчетности - метод GetOnDateRange

namespace LogSuite.Business.Repositories
{
    public class GisRepository : IGisRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public GisRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<GisDTO> Create(GisDTO dto)
        {
            Gis toDb = _mapper.Map<GisDTO, Gis>(dto);
            var result = await _db.Gises.AddAsync(toDb);
            await _db.SaveChangesAsync();
            return _mapper.Map<Gis, GisDTO>(result.Entity);
        }

        public async Task<int> Delete(int id)
        {
            var fromDb = await _db.Gises
                .Include(x => x.Names)
                .Include(x => x.Addons)
                .Include(x => x.GisInputNames)
                .Include(x => x.GisOutputNames)
                .Include(x => x.GisInputValues)
                .Include(x => x.GisOutputValues)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
            if (fromDb != null)
            {
                var parent = await _db.GisCountries
                    .Where(x => x.GisId == id)
                    .AnyAsync();
                if (parent)
                {
                    return -1;
                }
                else
                {
                    _db.Gises.Remove(fromDb);
                    return await _db.SaveChangesAsync();
                }
            }
            return 0;
        }

        public async Task<GisDTO> Get(int id)
        {
            var fromDb = await _db.Gises
                .Include(x => x.Names)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
            var dto = _mapper.Map<Gis, GisDTO>(fromDb);
            return dto;
        }

        public async Task<IEnumerable<GisDTO>> GetAll()
        {
            var entities = _db.Gises
                .Include(x => x.Names)
                .Include(x => x.GisInputNames)
                .Include(x => x.GisOutputNames)
                .Include(x => x.Addons).ThenInclude(a => a.Names)
                .Include(x => x.Countries.OrderBy(x => x.Country.Name)).ThenInclude(gc => gc.Country).ThenInclude(n => n.Names)
                .OrderBy(x => x.Name);
            IEnumerable<GisDTO> dtos = _mapper.Map<IEnumerable<Gis>, IEnumerable<GisDTO>>(entities);
            return dtos;
        }

        public async Task<List<GisDTO>> GetOnDateRange(DateTime startDate, DateTime finishDate)
        {
            var gises = await _db.Gises
                .Include(x => x.Countries).ThenInclude(c => c.Country)
                .Include(x => x.Addons)
                .ToListAsync();
            foreach (var gis in gises)
            {
                foreach (var country in gis.Countries)
                {
                    country.Values = await _db.GisCountryValues
                        .Where(x => x.GisCountryId == country.Id && x.DateReport >= startDate && x.DateReport <= finishDate)
                        .OrderBy(x => x.DateReport)
                        .ToListAsync();
                    var firstDate = new DateTime(startDate.Year, startDate.Month, 1);
                    var lastDate = new DateTime(finishDate.Year, finishDate.Month, 1);
                    country.Resources = await _db.GisCountryResources
                        .Where(x => x.GisCountryId == country.Id && x.Month >= firstDate && x.Month <= lastDate)
                        .OrderBy(x => x.Month)
                        .ToListAsync();
                }
                foreach (var addon in gis.Addons)
                {
                    addon.Values = await _db.GisAddonValues
                        .Where(x => x.GisAddonId == addon.Id && x.DateReport >= startDate && x.DateReport <= finishDate)
                        .OrderBy(x => x.DateReport)
                        .ToListAsync();
                }
                gis.GisInputValues = await _db.GisInputValues
                    .Where(x => x.GisId == gis.Id && x.DateReport >= startDate && x.DateReport <= finishDate)
                    .OrderBy(x => x.DateReport)
                    .ToListAsync();
                gis.GisOutputValues = await _db.GisOutputValues
                    .Where(x => x.GisId == gis.Id && x.DateReport >= startDate && x.DateReport <= finishDate)
                    .OrderBy(x => x.DateReport)
                    .ToListAsync();
            }
            List<GisDTO> dtos = _mapper.Map<IEnumerable<Gis>, List<GisDTO>>(gises);
            return dtos;
        }

        public async Task<PagedList<GisDTO>> GetPaged(Params parameters)
        {
            var source = _db.Gises
                    .Include(x => x.Names)
                    .AsQueryable();
            source = source.Search(parameters.Filter);
            source = source.Sort(parameters.Order, parameters.OrderAsc);
            var result = await PagedList<Gis>.ToPagedListAsync(source, parameters.PageNumber, parameters.PageSize);
            var entities = _mapper.Map<List<GisDTO>>(result);

            return new PagedList<GisDTO>(entities, result.MetaData);
        }

        public async Task<GisDTO> IsUnique(GisDTO dto, int id = 0)
        {
            if (id == 0)
            {
                var fromDb = await _db.Gises.Include(x => x.Names)
                    .Where(x => x.Names.Where(n => dto.Name.ToLower().Equals(n.Name.ToLower())).Any()
                        || x.Name.ToLower() == dto.Name.ToLower())
                    .FirstOrDefaultAsync();
                var result = _mapper.Map<Gis, GisDTO>(fromDb);
                return result;
            }
            else
            {
                var fromDb = await _db.Gises.Include(x => x.Names)
                    .Where(x => (x.Names.Where(n => dto.Name.ToLower().Equals(n.Name.ToLower())).Any()
                        || x.Name.ToLower() == dto.Name.ToLower())
                        && x.Id != id)
                    .FirstOrDefaultAsync();
                var result = _mapper.Map<Gis, GisDTO>(fromDb);
                return result;
            }
        }

        public async Task<GisDTO> Update(GisDTO dto)
        {
            var fromDb = await _db.Gises
                .FindAsync(dto.Id);
            var toUpdate = _mapper.Map(dto, fromDb);
            var updated = _db.Gises.Update(toUpdate);
            await _db.SaveChangesAsync();
            var result = _mapper.Map<Gis, GisDTO>(updated.Entity);
            return result;
        }
    }
}