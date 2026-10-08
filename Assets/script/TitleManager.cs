using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//public enum Scene
//{
//    TitleScene,
//    SelectScene,
//    SugorokuScene,
//    BattleScene
//}

public class TitleManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer back;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private GameObject canvas;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Camera cam;

    [SerializeField] private float targetSize;
    [SerializeField] private Vector3 target;
    private float timer = 0;
    private bool isClick = false;
    private bool isOpen = false;

    private int index = 0;
    private float animSpeed = 0.5f;
    private float zoomSpeed = 3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
        {
            isClick = true;
            canvas.SetActive(false);
        }

        if (isClick)
        {
            timer += Time.deltaTime;
            if (timer >= index * animSpeed)
            {
                if (index == sprites.Length - 1)
                {
                    isOpen = true;
                }
                else
                {
                    index++;
                    back.sprite = sprites[index];
                }
            }
        }

        if (isOpen)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, target, Time.deltaTime * zoomSpeed);
            if (cam.orthographicSize <= 0.75f)
                SceneManager.LoadScene("SelectScene");
        }
    }
}