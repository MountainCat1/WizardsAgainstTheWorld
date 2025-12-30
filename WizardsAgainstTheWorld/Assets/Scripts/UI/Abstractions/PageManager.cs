using UnityEngine;
using Zenject;

namespace UI.Abstractions
{
    public class PageManager : MonoBehaviour
    {
        [Inject] private IInputManager _inputManager;
        [Inject] private IUIManager _uiManager;

        public PageUI InitialPage => initialPage;

        [SerializeField] private PageUI initialPage;
        [SerializeField] private bool showInitialPageOnStart = true;

        private PageUI[] _pages;
        private PageUI _currentPage;

        private void Awake()
        {
            // Get all children of this object that are PageUI
            _pages = GetComponentsInChildren<PageUI>(true);

            // Hide all pages
            foreach (var page in _pages)
            {
                page.Hide();
            }

            // Show the initial page
            if (showInitialPageOnStart)
                ShowPage(initialPage);
        }

        public void ShowPage(PageUI page)
        {
            _uiManager.Show(page);
        }
    }
}