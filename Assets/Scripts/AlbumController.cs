using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlbumController : MonoBehaviour
{
    private string[] spritesPaths;
    private Sprite[] sprites;
    public SpriteRenderer spriteRenderer;
    public Button prevButton;
    public Button nextButton;
    public int spriteIndex;
    public bool IsLastSprite => spriteIndex == spritesPaths.Length - 1;
    public bool IsFirstSprite => spriteIndex == 0;
    private MaskAnimation _maskAnimation;
    public TMP_Text txt;
    private string imagesPath;

    private void Start()
    {
        BetterStreamingAssets.Initialize();
        _maskAnimation = GetComponentInChildren<MaskAnimation>();
        /*sprites = new List<Sprite>();*/

        LoadPathsImageFolder();

        sprites = new Sprite[imagesPath.Length];
        spriteIndex = 0; //Random.Range(0, spritesPaths.Length);
        UpdateSprite();
        CheckButtonsActive();
    }



    [ContextMenu("NextSprite")]
    public void NextSprite()
    {
        if (IsLastSprite) return;
        spriteIndex++;
        CheckIsLastSprite();
        UpdateSprite();
    }

    [ContextMenu("PrevSprite")]
    public void PrevSprite()
    {
        if (IsFirstSprite) return;
        spriteIndex--;
        CheckIsFirstSprite();
        UpdateSprite();
    }

    public void CheckIsFirstSprite()
    {
        prevButton.gameObject.SetActive(!IsFirstSprite);
        nextButton.gameObject.SetActive(true);
    }

    public void CheckIsLastSprite()
    {
        nextButton.gameObject.SetActive(!IsLastSprite);
        prevButton.gameObject.SetActive(true);

    }

    public void CheckButtonsActive()
    {
        CheckIsFirstSprite();
        CheckIsLastSprite();
    }

    // TODO: Cache images
    public void UpdateSprite()
    {
        print($"Sprite renderere {spriteRenderer}");
        spriteRenderer.sprite = LoadCurrentImage();//sprites[spriteIndex]; //GetSpritefromImage(spritesPaths[spriteIndex]);
        print($"spritesPaths[spriteIndex] {sprites[spriteIndex].name}");
        _maskAnimation.UpdateSpriteSize(spriteRenderer.sprite);
        /*  if (AdjustPictureSizeToReference)
          {
              var size = spriteRenderer.size;
              float ratio = size.x / size.y;
              Vector2 newSize = Vector2.one;
              if (size.x > size.y)
              {
                  newSize= new Vector2(size.x, size.x / ratio);
              }
              else
              {
                  newSize = new Vector2(size.y * ratio, size.y);
              }
              spriteRenderer.size = newSize;
              print($"Sprite: {spritesPaths[spriteIndex]}, initial size:{size.x} x {size.y}, ratio {ratio}, new size: {newSize.x} x {newSize.y}");
          }*/
        /* else
         {
             // If user didn't select pics to adjust to mask, then the sprite is passed so mask updates its size 
             _maskAnimation.UpdateSize(spriteRenderer.sprite);
         }*/

    }



    // CREDIT: https://forum.unity.com/threads/solved-loading-image-from-streamingassets.717869/#post-6622780
    private Sprite LoadSpritefromPath(string imgPath)
    {
        //Converts desired path into byte array
        byte[] pngBytes = Array.Empty<byte>();
        //Converts desired path into byte array
        if (imagesPath == "Default") pngBytes = BetterStreamingAssets.ReadAllBytes(imgPath);
        else pngBytes = File.ReadAllBytes(imgPath);

        //Creates texture and loads byte array data to create image
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(pngBytes);

        //Creates a new Sprite based on the Texture2D
        Sprite fromTex = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f),
            100.0f);
        print($"Tex: w {fromTex.texture.width} x h {fromTex.texture.height}");
        return fromTex;
    }

    public void LoadPathsImageFolder()
    {
        string supportedExtensions =
            "*.jpg,*.gif,*.png,*.bmp,*.jpe,*.jpeg,*.wmf,*.emf,*.xbm,*.ico,*.eps,*.tif,*.tiff,*.g01,*.g02,*.g03,*.g04,*.g05,*.g06,*.g07,*.g08";

        imagesPath = PlayerPrefs.GetString("PicturesFolder", "Default");
        if (imagesPath == "Default")
        {
            spritesPaths = BetterStreamingAssets.GetFiles("Pictures", "*.*", SearchOption.AllDirectories)
                .Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        }
        else
        {
            spritesPaths = System.IO.Directory.GetFiles(imagesPath, "*.*", SearchOption.AllDirectories)
                .Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        }

        /*print($"Found {spritesPaths.Length} images:");
        foreach (var fileInfo in spritesPaths)
        {
            print(fileInfo);
        }*/
        string text = $"Found {spritesPaths.Length} sprites\n";
        /*foreach (var spritesPath in spritesPaths)
        {
            text += spritesPath + ", ";
            StartCoroutine(CallGetSpriteFromImage(spritesPath));
        }*/
        print(text);
    }

    private Sprite LoadCurrentImage()
    {
        if (sprites[spriteIndex] == null)
        {
            var spritePath = spritesPaths[spriteIndex];
            print($"Loading image at position {spriteIndex}, path: {spritePath}");
            sprites[spriteIndex] = LoadSpritefromPath(spritePath);
        }
        return sprites[spriteIndex];
    }

    /*public IEnumerator CallGetSpriteFromImage(string imagePath)
    {
        print($"[CallGetSpriteFromImage] {imagePath}");
        sprites.Add(GetSpritefromImage(imagePath));
        yield return null;
    }*/
}