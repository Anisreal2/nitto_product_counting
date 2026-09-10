using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Models
{
    public class ProductModel : INotifyPropertyChanged
    {
        private int _id;
        private string _productId;
        private int _trayIndex;
        private ProductStatus _status;
        private string _imagePath;
        private string _overlayImagePath;
        private List<object> _results;
        private bool _isSelected;
        private bool _isPreData;

        private string _lotCode;
        private int _trayId;
        private int _pcsIndex;
        private DateTime? _inspectionTime;
        private string _trayCode;

        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
        public string ProductId
        {
            get
            {
                return _productId;
            }
            set
            {
                if (_productId != value)
                {
                    _productId = value;
                    OnPropertyChanged(nameof(ProductId));
                }
            }
        }

        public int TrayIndex
        {
            get
            {
                return _trayIndex;
            }
            set
            {
                if (_trayIndex != value)
                {
                    _trayIndex = value;
                    OnPropertyChanged(nameof(TrayIndex));
                }
            }
        }

        public ProductStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
                if (_imagePath != value)
                {
                    _imagePath = value;
                    OnPropertyChanged(nameof(ImagePath));
                }
            }
        }

        public string OverlayImagePath
        {
            get
            {
                return _overlayImagePath;
            }
            set
            {
                if (_overlayImagePath != value)
                {
                    _overlayImagePath = value;
                    OnPropertyChanged(nameof(OverlayImagePath));
                }
            }
        }

        public List<object> Results
        {
            get
            {
                return _results;
            }
            set
            {
                if (_results != value)
                {
                    _results = value;
                    OnPropertyChanged(nameof(Results));
                }
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public bool IsPreData
        {
            get
            {
                return _isPreData;
            }
            set
            {
                if (_isPreData != value)
                {
                    _isPreData = value;
                    OnPropertyChanged(nameof(IsPreData));
                }
            }
        }

        public string LotCode
        {
            get => _lotCode;
            set
            {
                if (_lotCode != value)
                {
                    _lotCode = value;
                    OnPropertyChanged(nameof(LotCode));
                }
            }
        }

        public int TrayId
        {
            get => _trayId;
            set
            {
                if (_trayId != value)
                {
                    _trayId = value;
                    OnPropertyChanged(nameof(TrayId));
                }
            }
        }

        public int PcsIndex
        {
            get => _pcsIndex;
            set
            {
                if (_pcsIndex != value)
                {
                    _pcsIndex = value;
                    OnPropertyChanged(nameof(PcsIndex));
                }
            }
        }

        public DateTime? InspectionTime
        {
            get => _inspectionTime;
            set
            {
                if (_inspectionTime != value)
                {
                    _inspectionTime = value;
                    OnPropertyChanged(nameof(InspectionTime));
                    OnPropertyChanged(nameof(InspectionTimeFormatted));
                }
            }
        }
        public string TrayCode
        {
            get => _trayCode;
            set
            {
                if (_trayCode != value)
                {
                    _trayCode = value;
                    OnPropertyChanged(nameof(TrayCode));
                }
            }
        }
        public string InspectionTimeFormatted
        {
            get => InspectionTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ProductStatus
    {
        None,
        OK,
        NG,
        PreOK,
        PreNG,
        Processing
    }
}
