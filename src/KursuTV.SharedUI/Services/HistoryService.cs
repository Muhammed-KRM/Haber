using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace KursuTV.SharedUI.Services;

/// <summary>
/// Uygulama içi dolaşım geçmişini takip ederek güvenilir bir "Geri" butonu deneyimi sunar.
/// </summary>
public class HistoryService : IDisposable
{
    private readonly NavigationManager _navigationManager;
    private readonly IJSRuntime _jsRuntime;
    private readonly List<string> _history = new();
    private bool _isNavigatingBack = false;

    public HistoryService(NavigationManager navigationManager, IJSRuntime jsRuntime)
    {
        _navigationManager = navigationManager;
        _jsRuntime = jsRuntime;
        
        _history.Add(_navigationManager.Uri);
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (_isNavigatingBack)
        {
            // Eğer kendimiz geri döndürüyorsak (NavigateTo ile), yeni sayfayı eklemeyiz.
            _isNavigatingBack = false;
        }
        else
        {
            // Aynı sayfaya tekrar gitmediyse listeye ekle
            if (_history.Count == 0 || _history.Last() != e.Location)
            {
                _history.Add(e.Location);
            }
        }
    }

    public async Task GoBackAsync()
    {
        if (_history.Count > 1)
        {
            // Mevcut sayfayı geçmişten çıkar
            _history.RemoveAt(_history.Count - 1);
            
            // Bir önceki sayfanın URL'sini al
            var previousUrl = _history.Last();
            
            _isNavigatingBack = true;
            _navigationManager.NavigateTo(previousUrl);
        }
        else
        {
            // Geçmiş yoksa (direkt link ile gelindiyse) anasayfaya gönder
            _navigationManager.NavigateTo("/");
        }
    }

    public bool CanGoBack => _history.Count > 1;

    public void Dispose()
    {
        _navigationManager.LocationChanged -= OnLocationChanged;
    }
}
