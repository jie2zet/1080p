using CMP.Constants.Plc.RobotControl;
using CMP.Services.Plc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WPFLocalizeExtension.Extensions;

namespace CMP.ViewModels.Common
{
    public class StationOpViewModelBase : TabContentViewModelBase
    {
        public readonly MitsubishiPlcService _plc;
        protected DispatcherTimer? _readTimer;
        protected bool _isReading;

        private string? _tabTitleKey;
        public string? TabTitleKey
        {
            get => _tabTitleKey;
            set
            {
                if (SetProperty(ref _tabTitleKey, value))
                {
                    RefreshTabTitle();
                }
            }
        }

        public void RefreshTabTitle()
        {
            if (!string.IsNullOrEmpty(TabTitleKey))
            {
                var loc = new LocExtension($"CMP:Resources:{TabTitleKey}");
                loc.ResolveLocalizedValue(out string? localizedValue);
                TabTitle = localizedValue ?? TabTitleKey;
            }
        }

        private string? _tabTitle;
        public new string? TabTitle
        {
            get => _tabTitle ?? base.TabTitle;
            set => SetProperty(ref _tabTitle, value);
        }

        private int _currentViewIndex = 0;
        public int CurrentViewIndex
        {
            get => _currentViewIndex;
            set => SetProperty(ref _currentViewIndex, value);
        }        

        #region 急停、报警复位、保存

        private bool _emergencyStopToggle;
        public bool EmergencyStopToggle
        {
            get => _emergencyStopToggle;
            set => SetProperty(ref _emergencyStopToggle, value);           
        }

        private bool _alarmResetToggle;
        public bool AlarmResetToggle
        {
            get => _alarmResetToggle;
            set => SetProperty(ref _alarmResetToggle, value);
        }

        private bool _saveToggle;
        public bool SaveToggle
        {
            get => _saveToggle;
            set => SetProperty(ref _saveToggle, value);
        }
        #endregion

        public StationOpViewModelBase(int id, string title) : base(id, title)
        {
            _plc = GlobalService.RobotControlPlc;
        }
    }
}
