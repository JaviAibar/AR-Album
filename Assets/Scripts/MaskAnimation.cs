using UnityEngine;

public class MaskAnimation : MonoBehaviour
{
    private SpriteMask mask;
    public Sprite[] sprites;
    private int index = 0;

    public int frames = 30;

    public float timer = 0;
    public float speed = 5;
    public bool BreakingEffectOn => PlayerPrefs.GetInt("BreakingAnimation", 0) == 1;

    private void Awake()
    {
        mask = GetComponent<SpriteMask>();
    }

    private void OnEnable()
    {
        index = 0;
    }

    private void Update()
    {
        if (!enabled) return;
        //  UpdateSize(localScale);
        //  print($"Breaking anim is set to {BreakingEffectOn}");
        if (!BreakingEffectOn)
        {
            mask.sprite = sprites[^1];
            return;
        }

        ChangeMaskSpriteEveryTimeUnit();
    }

    private void ChangeMaskSpriteEveryTimeUnit()
    {
        timer += Time.deltaTime * speed;
        if (timer < 1f) return;
        mask.sprite = sprites[index];
        index++;
        if (index == sprites.Length)
            index = 0;
        timer = 0;
    }

    public void UpdateSize(Vector3 newLocalScale)
    {
        transform.localScale = newLocalScale;
    }

    public void UpdateSpriteSize(Sprite spriteRendererSprite = null)
    {
        if (!spriteRendererSprite) return;
        Rect tex = spriteRendererSprite.textureRect;
        var maskTexture = sprites[0].texture;
        Vector3 newLocalScale = new Vector3(tex.width / maskTexture.width,
            tex.height / maskTexture.height, 1);

        UpdateSize(newLocalScale);
        print(
            $"LocalScale of Ref set to Vector2({newLocalScale} due to: tex.wid: {tex.width} / refWid {maskTexture.width}, tex.hei: {tex.height} / refHei {maskTexture.height})");
    }
}