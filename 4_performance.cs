public class performance
{
    //длительность выполнения 545 мс
    public static void Solution1(int year)
    {
        var rand = new Random(1);
        var receptions = Enumerable.Range(1, 500000).SelectMany(pid => Enumerable.Range(1, rand.Next(0, 100)).Select(rid => new { PatientId = pid, ReceptionStart = new DateTime(2017, 06, 30).AddDays(-rand.Next(1, 500)) })).ToList();
        var patients = Enumerable.Range(1, 500000).Select(pid => new { Id = pid, Surname = string.Format("Иванов{0}", pid) }).ToList();
        List<dynamic> result = new();
        HashSet<int> recs = new();
        foreach (var reception in receptions)
        {
            if (reception.ReceptionStart.Year < year)
                recs.Add(reception.PatientId);
        }
        foreach (var patient in patients)
        {
            if (recs.Contains(patient.Id))
                result.Add(patient);
        }
    }

    //длительность выполнения 532 мс
    public static void Solution2(int year)
    {
        var rand = new Random(1);
        var receptions = Enumerable.Range(1, 500000).SelectMany(pid => Enumerable.Range(1, rand.Next(0, 100)).Select(rid => new { PatientId = pid, ReceptionStart = new DateTime(2017, 06, 30).AddDays(-rand.Next(1, 500)) })).ToList();
        var patients = Enumerable.Range(1, 500000).Select(pid => new { Id = pid, Surname = string.Format("Иванов{0}", pid) }).ToList();
        List<dynamic> result = new();
        HashSet<int> recs = new();
        foreach (var reception in receptions)
        {
            if (reception.ReceptionStart.Year < year)
                recs.Add(reception.PatientId);
        }
        result = patients.Where(x => recs.Contains(x.Id)).ToList<dynamic>();
    }

    //длительность выполнения 614 мс
    public static void Solution3(int year)
    {
        var rand = new Random(1);
        var receptions = Enumerable.Range(1, 500000).SelectMany(pid => Enumerable.Range(1, rand.Next(0, 100)).Select(rid => new { PatientId = pid, ReceptionStart = new DateTime(2017, 06, 30).AddDays(-rand.Next(1, 500)) })).ToList();
        var patients = Enumerable.Range(1, 500000).Select(pid => new { Id = pid, Surname = string.Format("Иванов{0}", pid) }).ToList();
        HashSet<int> recs = receptions.Where(x => x.ReceptionStart.Year < year).Select(r =>  r.PatientId).ToHashSet<int>();
        List<dynamic> result = patients.Where(x => recs.Contains(x.Id)).ToList<dynamic>();
    }
}
