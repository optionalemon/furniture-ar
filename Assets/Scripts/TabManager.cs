using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private Button tabButton1;
    [SerializeField] private Button tabButton2;
    [SerializeField] private Button tabButton3;

    [Header("ScrollViews")]
    [SerializeField] private GameObject scrollView1;
    [SerializeField] private GameObject scrollView2;
    [SerializeField] private GameObject scrollView3;

    [Header("Button Colors")]
    [SerializeField] private Color activeTabColor = Color.blue;
    [SerializeField] private Color inactiveTabColor = Color.gray;

    private void Start()
    {
        // Initialize the tab system
        SetupTabListeners();
        
        // Open the first tab by default
        OpenTab(1);
    }

    private void SetupTabListeners()
    {
        // Add click listeners to tab buttons
        tabButton1.onClick.AddListener(() => OpenTab(1));
        tabButton2.onClick.AddListener(() => OpenTab(2));
        tabButton3.onClick.AddListener(() => OpenTab(3));
    }

    public void OpenTab(int tabIndex)
    {
        // Disable all ScrollViews first
        scrollView1.SetActive(false);
        scrollView2.SetActive(false);
        scrollView3.SetActive(false);

        // Reset all button colors
        ResetButtonColors();

        // Activate the selected ScrollView
        switch (tabIndex)
        {
            case 1:
                scrollView1.SetActive(true);
                SetButtonColor(tabButton1, activeTabColor);
                break;
            case 2:
                scrollView2.SetActive(true);
                SetButtonColor(tabButton2, activeTabColor);
                break;
            case 3:
                scrollView3.SetActive(true);
                SetButtonColor(tabButton3, activeTabColor);
                break;
        }
    }

    private void ResetButtonColors()
    {
        // Reset all button colors to inactive
        SetButtonColor(tabButton1, inactiveTabColor);
        SetButtonColor(tabButton2, inactiveTabColor);
        SetButtonColor(tabButton3, inactiveTabColor);
    }

    private void SetButtonColor(Button button, Color color)
    {
        // Change the button's color (assuming you're using Image component)
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }
}