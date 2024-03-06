// See https://youtu.be/gpaq5bAjya8  for accompanying tutorial and usage!

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.AR;

[RequireComponent(typeof(ARTrackedImageManager))]
public class PlaceTrackedImages : MonoBehaviour
{
    private ARTrackedImageManager _trackedImagesManager;

    private ARGestureInteractor _gestureInteractor;
    //    public XRReferenceImageLibrary imageLibrary;
    private string referenceImagesPath;

    // public TMPro.TMP_Text debug;

    // List of prefabs to instantiate - these should be named the same
    // as their corresponding 2D images in the reference image library 
    public GameObject[] arPrefabs;

    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();


    void OnDisable()
    {
        _trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private IEnumerator Start()

    {
        BetterStreamingAssets.Initialize();
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();
        yield return new WaitForSeconds(3);
        if (arPrefabs.Length == 0) throw new Exception("There is no prefab to set. Application cannot work");
        _trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
        LoadReferenceImages();
    }

    public void LoadReferenceImages()
    {
        // print("Trying to load");
        string supportedExtensions =
            "*.jpg,*.gif,*.png,*.bmp,*.jpe,*.jpeg,*.wmf,*.emf,*.xbm,*.ico,*.eps,*.tif,*.tiff,*.g01,*.g02,*.g03,*.g04,*.g05,*.g06,*.g07,*.g08";
        referenceImagesPath = PlayerPrefs.GetString("ReferencesFolder", "Default");
        string[] paths = Array.Empty<string>();
        // _trackedImagesManager.enabled = false;
        //  print($"Path is {referenceImagesPath}");
        if (referenceImagesPath == "Default")
        {
            // print("Searching in ImageReferences");
            paths = BetterStreamingAssets.GetFiles("ImageReferences", "*.*", SearchOption.AllDirectories)
                .Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            string filesFound = $"Found {paths.Length} files:";
            foreach (var s in paths)
            {
                filesFound += $"\t{s}";
            }
            // print(filesFound);
            // print($"Can I Add images? {_trackedImagesManager.descriptor.supportsMutableLibrary}");
        }
        else
        {
            DirectoryInfo dataDir = new DirectoryInfo(referenceImagesPath);
            try
            {
                paths = dataDir.GetFiles().Select(e => e.FullName).ToArray();
                string filesFound = $"Found {paths.Length} files:";
                foreach (var s in paths)
                {
                    filesFound += $"\t{s}";
                }
                //  print(filesFound);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }

        if (paths.Length == 0) print("No images found");
        foreach (string imgPath in paths)
        {
            StartCoroutine(ProcessImage(imgPath));
        }

        _trackedImagesManager.enabled = true;
    }

    public IEnumerator ProcessImage(string imgPath)
    {
        // CREDITS: https://www.youtube.com/watch?v=XE4jSy_D_vM&ab_channel=DilmerValecillos | https://github.com/dilmerv/UnityARFoundationEssentials
        Texture2D tex = GetTexturefromPath(imgPath);
        if (_trackedImagesManager.referenceLibrary is not MutableRuntimeReferenceImageLibrary
            mutableRuntimeReferenceImageLibrary) yield break;
        AddReferenceImageJobState addReferenceImageJobState =
            mutableRuntimeReferenceImageLibrary.ScheduleAddImageWithValidationJob(tex,
                imgPath, 0.1f);
        JobHandle jobHandle = addReferenceImageJobState.jobHandle;
        //    print("Job started!");


        while (!jobHandle.IsCompleted)
        {
            //      print("Job Running...");
            yield return null;
        }


        //PrintInfo(addReferenceImageJobState, mutableRuntimeReferenceImageLibrary, imgPath, tex);
    }

    private void PrintInfo(AddReferenceImageJobState addReferenceImageJobState, MutableRuntimeReferenceImageLibrary mutableRuntimeReferenceImageLibrary, string imgPath, Texture2D tex)
    {
        print(
                $"is my texture ({imgPath}) supported ({tex.format})? {mutableRuntimeReferenceImageLibrary.IsTextureFormatSupported(tex.format)}");
        print(
            $"Job finished with result {addReferenceImageJobState.status.ToString()}!({mutableRuntimeReferenceImageLibrary.count})");
        print($"Imagenes de referencia: {_trackedImagesManager.referenceLibrary.count}");
        print($"trackImageManager.trackables.count ({_trackedImagesManager.trackables.count})");
        print($"_trackedImagesManager.referenceLibrary[0].name ({_trackedImagesManager.referenceLibrary[0].name})");
        print($"_trackedImagesManager.referenceLibrary[0].size.x ({_trackedImagesManager.referenceLibrary[0].size.x})");
        print($"_trackedImagesManager.referenceLibrary[0].size.y ({_trackedImagesManager.referenceLibrary[0].size.y})");
        // print($"trackImageManager.trackedImagePrefab.name ({_trackedImagesManager.trackedImagePrefab.name})"); Not interesting bc using arPrefabs array instead

    }


    // Event Handler
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Loop through all new tracked images that have been detected
        foreach (var trackedImage in eventArgs.added)
        {
            // debug.text = $"Added! {Time.time}\n{eventArgs.added.Count}";
            //    eventArgs.added.ToList().ForEach(e => debug.text += e.referenceImage.name);
            // Get the name of the reference image
            var imageName = trackedImage.referenceImage.name;
            //   print($"trackedImage.referenceImage.size: {trackedImage.referenceImage.size.x} x {trackedImage.referenceImage.size.y}");
            foreach (var curPrefab in arPrefabs)
            {
                // Removed some code to simplify the app, not interested in differente prefab instantiation
                /*if (string.Compare(curPrefab.name, imageName, StringComparison.OrdinalIgnoreCase) == 0
                    && !_instantiatedPrefabs.ContainsKey(imageName))
                {*/
                if (!_instantiatedPrefabs.ContainsKey(imageName))
                {
                    // Instantiate the prefab, parenting it to the ARTrackedImage
                    var newPrefab = Instantiate(curPrefab, trackedImage.transform);
                    // print($"Instantiated {curPrefab.name}");
                    // Add the created prefab to our array
                    //newPrefab.transform.localScale *= PlayerPrefs.GetFloat("PrefabScale", 1);
                    /*Vector3 localScale = new Vector3(PlayerPrefs.GetFloat("PrefabScaleX", 0.1f),
                        PlayerPrefs.GetFloat("PrefabScaleY", 0.1f), 1);
                    newPrefab.transform.GetChild(0).localScale = localScale;
                    print($"Retrieved img localScale from memo to ({localScale.x}, {localScale.y}, 1)");*/

                    _instantiatedPrefabs[imageName] = newPrefab;
                }
                //   print($"ADDED _trackedImagesManager.trackables.count {_trackedImagesManager.trackables.count}");
            }
        }

        // For all prefabs that have been created so far, set them active or not depending
        // on whether their corresponding image is currently being tracked
        foreach (var trackedImage in eventArgs.updated)
        {
            //  debug.text = $"Updated! {Time.time}\n{eventArgs.updated.Count}";
            //     eventArgs.updated.ToList().ForEach(e => debug.text += e.referenceImage.name);
            _instantiatedPrefabs[trackedImage.referenceImage.name]
                .SetActive(trackedImage.trackingState == TrackingState.Tracking);
            //  print($"UPDATED _trackedImagesManager.trackables.count {_trackedImagesManager.trackables.count}");
        }

        // If the AR subsystem has given up looking for a tracked image
        foreach (var trackedImage in eventArgs.removed)
        {
            Destroy(_instantiatedPrefabs[trackedImage.referenceImage.name]);
            _instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
        }


    }

    private Texture2D GetTexturefromPath(string imgPath)
    {
        byte[] pngBytes = Array.Empty<byte>();

        if (referenceImagesPath == "Default") pngBytes = BetterStreamingAssets.ReadAllBytes(imgPath);
        else pngBytes = File.ReadAllBytes(imgPath);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(pngBytes);
        print($"[{imgPath}] {tex.width} x {tex.height}");
        return tex;
    }
}