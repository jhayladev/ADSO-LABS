namespace AdsoLabs.Web.Services
{
    public class FichaStateService
    {
        public event Action? OnChange;

        public void NotifyChange()
        {
            OnChange?.Invoke();
        }
    }
}
