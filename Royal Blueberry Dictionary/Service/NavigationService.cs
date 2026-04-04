using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Royal_Blueberry_Dictionary.Service
{
    public class NavigationService
    {
        private readonly Dictionary<Type, object> _viewModelCache = new();
        private readonly IServiceProvider _sp;
        private Frame _mainFrame; 
        public NavigationService(IServiceProvider sp)
        {
            _sp = sp;
        }   
        public void NavigateTo<View,ViewModel>(object param) where View : Page 
                                                             where ViewModel : class 
        {
            if (!_viewModelCache.TryGetValue(typeof(ViewModel) , out var viewModel)) 
            {
                _viewModelCache[typeof(ViewModel)] = viewModel = _sp.GetRequiredService<ViewModel>();   
            }
            var view = _sp.GetRequiredService<View>();  
            view.DataContext = viewModel;
            if (viewModel is INavigationAware nav) nav.OnNavigatedTo(param);
            _mainFrame.Navigate(view);  
        }
        public void ClearCache<ViewModel>() => _viewModelCache.Remove(typeof(ViewModel));   

        public void GoBack() 
        {
            if (_mainFrame.CanGoBack) 
            {
                _mainFrame.GoBack();    
            }
        }
        public void GoForward() 
        {
            if (_mainFrame.CanGoForward) 
            {
                _mainFrame.GoForward();    
            }
        }
        public void SetMainFrame(Frame frame) => _mainFrame = frame;   
    }
    public interface INavigationAware
    {
        void OnNavigatedTo(object parameter);
        void OnNavigatedFrom(); 
    }
}
