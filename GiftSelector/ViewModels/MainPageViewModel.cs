using Plugin.Media;
using Plugin.Media.Abstractions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace GiftSelector.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private IPageDialogService _dialog;

        public MainPageViewModel(INavigationService navigationService,
            IPageDialogService dialog)
            : base(navigationService)
        {
            try
            {
                _dialog = dialog;

                IsBusy = false;

                Title = "Случайный подарко-выбиратель";

                AimColor = "#D8315B";
                AimSize = 1;

                TakePhotoCommand = new DelegateCommand(async () => await ExecuteTakePhoto());
                ChangePositionCommand = new DelegateCommand(ExecuteChangePosition);
                ChangeColorCommand = new DelegateCommand<string>(ExecuteChangeColor);
                ChangeSizeCommand = new DelegateCommand<string>(ExecuteChangeSize);
            }
            catch(Exception ex)
            {

            }
        }

        #region Bindable
        private Rectangle _randomPlace;
        public Rectangle RandomPlace
        {
            get => _randomPlace;
            set => SetProperty(ref _randomPlace, value);
        }

        private Rectangle _randomX;
        public Rectangle RandomX
        {
            get => _randomX;
            set => SetProperty(ref _randomX, value);
        }

        private Rectangle _randomY;
        public Rectangle RandomY
        {
            get => _randomY;
            set => SetProperty(ref _randomY, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private ImageSource _giftPhoto;
        public ImageSource GiftPhoto
        {
            get => _giftPhoto;
            set => SetProperty(ref _giftPhoto, value);
        }

        private string _aimColor;
        public string AimColor
        {
            get => _aimColor;
            set => SetProperty(ref _aimColor, value);
        }

        private double _aimSize;
        public double AimSize
        {
            get { return _aimSize; }
            set { _aimSize = value; }
        }
        #endregion

        public DelegateCommand TakePhotoCommand { get; set; }
        public DelegateCommand ChangePositionCommand { get; set; }
        public DelegateCommand<string> ChangeColorCommand { get; set; }
        public DelegateCommand<string> ChangeSizeCommand { get; set; }

        private async Task ExecuteTakePhoto()
        {
            IsBusy = true;
            try
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await _dialog.DisplayAlertAsync("No Camera", ":( No camera available.", "OK");
                }
                else
                {
                    MediaFile file = await CrossMedia.Current.TakePhotoAsync(
                        new StoreCameraMediaOptions
                        {
                            Directory = "Gifts",
                            Name = "gift.jpg"
                        }
                    );

                    if (file != null)
                    {
                        GiftPhoto = ImageSource.FromStream(() =>
                        {
                            Stream stream = file.GetStream();
                            return stream;
                        });
                    }

                    RandomPlace = GetRandomPlace();
                    RandomX = GetRandomX();
                    RandomY = GetRandomY();
                }
            }
            catch(Exception ex)
            {
                await _dialog.DisplayAlertAsync("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExecuteChangePosition()
        {
            RandomX = GetRandomX();
            RandomY = GetRandomY();
        }

        public void ExecuteChangeColor(string color) => AimColor = color;

        public void ExecuteChangeSize(string size)
        {
            double dSize = Convert.ToDouble(size);

            AimSize = dSize;

            _randomX.Width = dSize;
            RaisePropertyChanged(nameof(RandomX));
            _randomY.Height = dSize;
            RaisePropertyChanged(nameof(RandomY));
        }

        #region private / util
        private Rectangle GetRandomX()
        {
            return new Rectangle(new Random().NextDouble(), 0, AimSize, 1);
        }

        private Rectangle GetRandomY()
        {
            return new Rectangle(0, new Random().NextDouble(), 1, AimSize);
        }

        private Rectangle GetRandomPlace()
        {
            Random r = new Random();
            double x = r.NextDouble();
            double y = r.NextDouble();

            return new Rectangle(x, y, 55, 55);
        }
        #endregion
    }
}
