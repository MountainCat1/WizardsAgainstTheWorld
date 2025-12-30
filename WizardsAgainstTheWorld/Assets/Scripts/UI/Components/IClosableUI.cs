namespace UI
{
    public interface IClosableUI
    {
        void Hide();
        void Show();
        bool IsOpen { get; }
    }
}