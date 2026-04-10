using BlueBerryDictionary.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Royal_Blueberry_Dictionary.View.Pages;
using Royal_Blueberry_Dictionary.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Royal_Blueberry_Dictionary.Service
{
    public class NavigationService
    {
        private readonly Dictionary<Type, object> viewModelCache = new();
        private readonly IServiceProvider serviceProvider;
        private Frame? mainFrame;

        public NavigationService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void NavigateTo<TView, TViewModel>(object? parameter) where TView : Page
                                                                 where TViewModel : class
        {
            if (mainFrame == null)
            {
                throw new InvalidOperationException("Main frame has not been initialized.");
            }

            if (!viewModelCache.TryGetValue(typeof(TViewModel), out var viewModel))
            {
                viewModel = serviceProvider.GetRequiredService<TViewModel>();
                viewModelCache[typeof(TViewModel)] = viewModel;
            }

            var view = serviceProvider.GetRequiredService<TView>();
            view.DataContext = viewModel;

            if (viewModel is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(parameter!);
            }

            mainFrame.Navigate(view);
        }

        public void ClearCache<TViewModel>()
        {
            viewModelCache.Remove(typeof(TViewModel));
        }

        public void GoBack()
        {
            if (mainFrame?.CanGoBack == true)
            {
                mainFrame.GoBack();
            }
        }

        public void GoForward()
        {
            if (mainFrame?.CanGoForward == true)
            {
                mainFrame.GoForward();
            }
        }

        public void SetMainFrame(Frame frame)
        {
            mainFrame = frame;
        }

        public void NavigateByTag(string? tag)
        {
            switch (tag)
            {
                case "Home":
                    NavigateTo<HomePage, SearchViewModel>("home");
                    break;
                case "History":
                    NavigateTo<HistoryPage, HistoryPageViewModel>(null);
                    break;
                case "Favourite":
                    NavigateTo<FavouriteWordsPage, FavouriteWordsPageViewModel>(null);
                    break;
                case "MyWords":
                    NavigateTo<MyWordsPage, MyWordsPageViewModel>(null);
                    break;
                case "Account":
                    NavigateTo<AccountPage, AccountPageViewModel>(null);
                    break;
                case "Setting":
                    NavigateTo<SettingsPage, SettingsPageViewModel>(null);
                    break;
            }
        }
    }

    public interface INavigationAware
    {
        void OnNavigatedTo(object parameter);
        void OnNavigatedFrom();
    }
}
