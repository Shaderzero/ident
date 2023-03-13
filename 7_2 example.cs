//запрос документов с сортировкой и фильтрацией (в следующем примере улучшенный подход)

[HttpGet()]
public async Task<IActionResult> GetDrafts([FromQuery]Params parameters)
{
    var source = _context.Drafts
        .Include(d => d.Author)
        .Include(d => d.IncidentType)
        .Include(d => d.Department)
        .AsQueryable();
    switch (parameters.Status)
    {
        case "any":
            source = source.Where(s => !s.Status.Equals("close") && !s.Status.Equals("example"));
            break;
        default:
            source = source.Where(s => s.Status.Equals(parameters.Status));
            break;
    }
    if (!parameters.Status.Equals("example"))
    {
        switch (parameters.Type)
        {
            case "any":
                break;
            case "forAdmin":
                break;
            case "forUser":
                source = source.Where(s => s.AuthorId == parameters.AccountId);
                break;
            case "forRC":
                source = source.Where(s => (!s.Status.Equals("draft") && s.DepartmentId == parameters.DepartmentId) || (s.AuthorId == parameters.AccountId));
                break;
            case "forRM":
                source = source.Where(s => (!s.Status.Equals("draft")) || (s.DepartmentId == parameters.DepartmentId) || (s.AuthorId == parameters.AccountId));
                break;
        }
    }
    if (!string.IsNullOrEmpty(parameters.Order))
    {
        if (parameters.OrderAsc)
        {
            switch (parameters.Order)
            {
                case "dateCreate":
                    source = source.OrderBy(s => s.DateCreate);
                    break;
                case "description1":
                    source = source.OrderBy(s => s.Description1);
                    break;
                case "description2":
                    source = source.OrderBy(s => s.Description2);
                    break;
                case "author":
                    source = source.OrderBy(s => s.Author.Name);
                    break;
                case "department":
                    source = source.OrderBy(s => s.Department.Name);
                    break;
                case "status":
                    source = source.OrderBy(s => s.Status);
                    break;
                case "type":
                    source = source.OrderBy(s => s.IncidentType.Name);
                    break;
            }
        }
        else
        {
            switch (parameters.Order)
            {
                case "dateCreate":
                    source = source.OrderByDescending(s => s.DateCreate);
                    break;
                case "description1":
                    source = source.OrderByDescending(s => s.Description1);
                    break;
                case "description2":
                    source = source.OrderByDescending(s => s.Description2);
                    break;
                case "author":
                    source = source.OrderByDescending(s => s.Author.Name);
                    break;
                case "department":
                    source = source.OrderByDescending(s => s.Department.Name);
                    break;
                case "status":
                    source = source.OrderByDescending(s => s.Status);
                    break;
                case "type":
                    source = source.OrderByDescending(s => s.IncidentType);
                    break;
            }
        }
    }
    else
    {
        source = source.OrderBy(s => s.DateCreate);
    }
    if (!string.IsNullOrEmpty(parameters.Filter))
    {
        var f = parameters.Filter.ToLower();
        source = source.Where(s => s.Description1.ToLower().Contains(f)
                || s.Description2.ToLower().Contains(f)
                || s.Description3.ToLower().Contains(f)
                || s.Description4.ToLower().Contains(f)
                || s.Description5.ToLower().Contains(f)
                || s.Author.Name.ToLower().Contains(f)
                || s.Department.Name.ToLower().Contains(f)
                || s.Status.ToLower().Contains(f)
                || s.IncidentType.Name.ToLower().Contains(f)
                );
    }
    var result = await PagedList<Draft>.CreateAsync(source, parameters.PageNumber, parameters.PageSize);
    var drafts = _mapper.Map<IEnumerable<DraftForListDto>>(result);
    Response.AddPagination(result.CurrentPage, result.PageSize, result.TotalCount, result.TotalPages);
    return Ok(drafts);
}