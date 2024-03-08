using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private TMP_Dropdown langDropdown;
    [SerializeField] private GameObject debug;
    [SerializeField] private PlaceTrackedImages placeTrackedImages;
    [SerializeField] private GameObject selectRefImgsWindow;
    [SerializeField] private GameObject selectImgsWindow;
    [SerializeField] private Toggle breakingAnimationToggle;
    public bool IsDefaultRefImgs => PlayerPrefs.GetString("ReferencesFolder", "Default") == "Default";
    public bool IsDefaultImgs => PlayerPrefs.GetString("PicturesFolder", "Default") == "Default";

    private void OnDisable()
    {
        breakingAnimationToggle.onValueChanged.RemoveListener(BreakingAnimationToggleChanged);
    }
    IEnumerator Start()
    {
        // On Start and not OnEnable because it would still be null
        breakingAnimationToggle.onValueChanged.AddListener(BreakingAnimationToggleChanged);
        LanguageInitialization();
        ActivateInitializationWindows();
        breakingAnimationToggle.isOn = PlayerPrefs.GetInt("BreakingAnimation", 0) == 1;
        yield return new WaitForSeconds(2);
        placeTrackedImages = GetComponent<PlaceTrackedImages>();
        print($"Place track: {placeTrackedImages}");
    }

    private void ActivateInitializationWindows()
    {
        selectRefImgsWindow.SetActive(IsDefaultRefImgs);
        selectImgsWindow.SetActive(IsDefaultImgs);
    }

    private IEnumerator LanguageInitialization()
    {
        // Wait for the localization system to initialize
        yield return LocalizationSettings.InitializationOperation;

        // Generate list of available Locales
        var options = new List<TMP_Dropdown.OptionData>();
        int selected = 0;
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales[i];
            if (LocalizationSettings.SelectedLocale == locale)
                selected = i;
            options.Add(new TMP_Dropdown.OptionData(locale.name.Split(' ')[0]));
        }
        langDropdown.options = options;

        langDropdown.value = selected;
        langDropdown.onValueChanged.AddListener(LocaleSelected);
    }

    public void BreakingAnimationToggleChanged(bool value)
    => PlayerPrefs.SetInt("BreakingAnimation", Convert.ToInt32(value));

    public void Quit() => Application.Quit();

    public void SwitchVisibility() => menu.SetActive(!menu.activeSelf);

    public void SwitchDebug() => debug.SetActive(!debug.activeSelf);

    static void LocaleSelected(int index)
    => LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];

    public void OpenRefBrowser()
    => StartCoroutine(ShowLoadRefDialogCoroutine());

    public void OpenPicBrowser()
    => StartCoroutine(ShowLoadPicDialogCoroutine());

    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey("ReferencesFolder");
        PlayerPrefs.DeleteKey("PicturesFolder");
        PlayerPrefs.DeleteKey("BreakingAnimation");
        print("Removed memo of folders (refs and picts) and BreakingAnimation");
        RemoveSizeKeys();
    }

    public void RemoveSizeKeys()
    {
        PlayerPrefs.DeleteKey("LocalScale");
        print("Removed LocalScale");
    }

    IEnumerator ShowLoadRefDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, null, null,
            "Load Reference Image Folder", "Load");

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
                Debug.Log(FileBrowser.Result[i]);
            PlayerPrefs.SetString("ReferencesFolder", FileBrowser.Result[0]);
            placeTrackedImages.LoadReferenceImages();
            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            //byte[] bytes = FileBrowserHelpers.ReadBytesFromFile( FileBrowser.Result[0] );

            // Or, copy the first file to persistentDataPath
            //  string destinationPath = Path.Combine( Application.persistentDataPath, FileBrowserHelpers.GetFilename( FileBrowser.Result[0] ) );
            // FileBrowserHelpers.CopyFile( FileBrowser.Result[0], destinationPath );
        }
    }

    IEnumerator ShowLoadPicDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, null, null, "Load Image Folder",
            "Load");

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
                Debug.Log(FileBrowser.Result[i]);
            PlayerPrefs.SetString("PicturesFolder", FileBrowser.Result[0]);

            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            //byte[] bytes = FileBrowserHelpers.ReadBytesFromFile( FileBrowser.Result[0] );

            // Or, copy the first file to persistentDataPath
            //  string destinationPath = Path.Combine( Application.persistentDataPath, FileBrowserHelpers.GetFilename( FileBrowser.Result[0] ) );
            // FileBrowserHelpers.CopyFile( FileBrowser.Result[0], destinationPath );
        }
    }
}