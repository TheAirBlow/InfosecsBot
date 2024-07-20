using static InfosecsBot.Configuration;

namespace InfosecsBot;

public class ObedManager {
    public static readonly List<Obed> Obeds = [
        new Obed(new TimeSpan(8, 30, 0), new TimeSpan(9, 00, 0)),
        new Obed(new TimeSpan(12, 30, 0), new TimeSpan(13, 00, 0)),
        new Obed(new TimeSpan(16, 00, 0), new TimeSpan(16, 30, 0)),
        new Obed(new TimeSpan(19, 00, 0), new TimeSpan(19, 30, 0)),
        new Obed(new TimeSpan(21, 00, 0), new TimeSpan(21, 30, 0)),
    ];

    public static DateTime CurrentTime => DateTime.UtcNow.AddHours(Config.Timezone);

    public static DateTime ClosestObed => Obeds.Select(x => x.StartDate).MinBy(x => x - CurrentTime);

    public static Obed? CurrentObed => Obeds.FirstOrDefault(x => CurrentTime > x.StartDate && CurrentTime < x.EndDate);
}

public class Obed {
    public TimeSpan StartTime { get; }
    
    public TimeSpan EndTime { get; }
    
    public DateTime StartDate => ObedManager.CurrentTime.Date.Add(StartTime)
        .AddDays(ObedManager.CurrentTime.TimeOfDay > StartTime ? 1 : 0);
    
    public DateTime EndDate => ObedManager.CurrentTime.Date.Add(EndTime)
        .AddDays(ObedManager.CurrentTime.TimeOfDay > EndTime ? 1 : 0);

    public Obed(TimeSpan start, TimeSpan end) {
        StartTime = start; EndTime = end;
    }
}