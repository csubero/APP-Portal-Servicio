using PortalServicio.Models;
using System;

namespace PortalServicio.ViewModels
{
    public class TechnicianRegistryViewModel : BaseViewModel
    {
        private int _SQLiteRecordId;
        private Guid _InternalId;
        private TechnicianViewModel _Technician;
        private int _TechnicianId;
        private int _CDTTicketId;
        private DateTime _Started;
        private DateTime _Finished;
        private bool _IsExtendingToNextDay;
        private bool _IsExtended;
        private double _HoursNormal;
        private double _HoursNormalNight;
        private double _HoursDaytimeExtra;
        private double _HoursNightExtra;
        private double _HoursHolydayDaytime;
        private double _HoursHolydayNight;
        public double _HoursOffdayDaytime;
        private double _HoursOffdayNight;
        private double _HoursOffdayDaytimeExtra;
        private double _HoursOffdayNightExtra;
        private bool _IsDatetimeSet;

        public int SQLiteRecordId { get { return _SQLiteRecordId; } set { SetValue(ref _SQLiteRecordId, value); } }
        public int CDTTicketId { get { return _CDTTicketId; } set { SetValue(ref _CDTTicketId, value); } }
        public Guid InternalId { get { return _InternalId; } set { SetValue(ref _InternalId, value); } }
        public TechnicianViewModel Technician { get { return _Technician; } set { SetValue(ref _Technician, value); } }
        public int TechnicianId { get { return _TechnicianId; } set { SetValue(ref _TechnicianId, value); } }
        public DateTime Started { get { return _Started; } set { SetValue(ref _Started, value); IsExtendingToNextDay = Finished.TimeOfDay < value.TimeOfDay; } }
        public DateTime Finished { get { return _Finished; } set { SetValue(ref _Finished, value); IsExtendingToNextDay = value.TimeOfDay < Started.TimeOfDay; } }
        public bool IsExtendingToNextDay { get { return _IsExtendingToNextDay; } set { SetValue(ref _IsExtendingToNextDay, value); } }
        public bool IsExtended { get { return _IsExtended; } set { SetValue(ref _IsExtended, value); } }
        public double HoursNormal { get { return _HoursNormal; } set { SetValue(ref _HoursNormal, value); } }
        public double HoursNormalNight { get { return _HoursNormalNight; } set { SetValue(ref _HoursNormalNight, value); } }
        public double HoursDaytimeExtra { get { return _HoursDaytimeExtra; } set { SetValue(ref _HoursDaytimeExtra, value); } }
        public double HoursNightExtra { get { return _HoursNightExtra; } set { SetValue(ref _HoursNightExtra, value); } }
        public double HoursHolydayDaytime { get { return _HoursHolydayDaytime; } set { SetValue(ref _HoursHolydayDaytime, value); } }
        public double HoursHolydayNight { get { return _HoursHolydayNight; } set { SetValue(ref _HoursHolydayNight, value); } }
        public double HoursOffdayDaytime { get { return _HoursOffdayDaytime; } set { SetValue(ref _HoursOffdayDaytime, value); } }
        public double HoursOffdayNight { get { return _HoursOffdayNight; } set { SetValue(ref _HoursOffdayNight, value); } }
        public double HoursOffdayDaytimeExtra { get { return _HoursOffdayDaytimeExtra; } set { SetValue(ref _HoursOffdayDaytimeExtra, value); } }
        public double HoursOffdayNightExtra { get { return _HoursOffdayNightExtra; } set { SetValue(ref _HoursOffdayNightExtra, value); } }
        public bool IsDatetimeSet { get { return _IsDatetimeSet; } set { SetValue(ref _IsDatetimeSet, value); } }

        #region Constructors
        public TechnicianRegistryViewModel(TechnicianRegistry registry)
        {
            if (registry == null)
                return;
            InternalId = registry.InternalId;
            SQLiteRecordId = registry.SQLiteRecordId;
            CDTTicketId = registry.CDTTicketId;
            Started = registry.Started;
            Finished = registry.Finished;
            HoursDaytimeExtra = registry.HoursDaytimeExtra;
            HoursHolydayDaytime = registry.HoursHolydayDaytime;
            HoursHolydayNight = registry.HoursHolydayNight;
            HoursNightExtra = registry.HoursNightExtra;
            HoursNormal = registry.HoursNormal;
            HoursNormalNight = registry.HoursNormalNight;
            HoursOffdayDaytime = registry.HoursOffdayDaytime;
            HoursOffdayDaytimeExtra = registry.HoursOffdayDaytimeExtra;
            HoursOffdayNight = registry.HoursOffdayNight;
            HoursOffdayNightExtra = registry.HoursOffdayNightExtra;
            Technician = new TechnicianViewModel(registry.Technician);
            TechnicianId = registry.TechnicianId;
            IsDatetimeSet = registry.IsDatetimeSet;
        }

        public TechnicianRegistry ToModel()=>
            new TechnicianRegistry
            {
                InternalId = InternalId,
            SQLiteRecordId = SQLiteRecordId,
            CDTTicketId = CDTTicketId,
            Started = Started,
            Finished = Finished,
            HoursDaytimeExtra = HoursDaytimeExtra,
            HoursHolydayDaytime = HoursHolydayDaytime,
            HoursHolydayNight = HoursHolydayNight,
            HoursNightExtra = HoursNightExtra,
            HoursNormal = HoursNormal,
            HoursNormalNight = HoursNormalNight,
            HoursOffdayDaytime = HoursOffdayDaytime,
            HoursOffdayDaytimeExtra = HoursOffdayDaytimeExtra,
            HoursOffdayNight = HoursOffdayNight,
            HoursOffdayNightExtra = HoursOffdayNightExtra,
            Technician = Technician?.ToModel(),
            TechnicianId = TechnicianId,
            IsDatetimeSet = IsDatetimeSet
        };
        #endregion
    }
}
