using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] public LayerMask wallLayer;
    [SerializeField] private float raycastDistance = 0.225f;
    [SerializeField] private Vector2 boxCastSize = Vector2.one * 0.75f;
    
    private Vector2 _currentDirection = Vector2.zero;
    private Vector2 _queuedDirection = Vector2.zero;
    private Rigidbody2D _rb;
    private bool _isPacman = false;

    private Coroutine speedCoroutine = null;
    private Coroutine cloneCoroutine = null;
    private Coroutine freezeCoroutine = null;
    private Coroutine fearCoroutine = null;
    private Coroutine visionCoroutine = null;
    private int visionHacksLeft = 3;
    private int speedHacksLeft = 3;
    private int cloneHacksLeft = 3;
    private int freezeHacksLeft = 2;
    private int fearHacksLeft = 1;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if(GameSettings.instance.selectedCharacter == "pacman")
            _isPacman = true;
    }

    private void Start()
    {
        wallLayer = LayerMask.GetMask("Obstacle");
    }

    private void Update()
    {
        HandleInput();
        TryQueuedDirection();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void HandleInput()
    {
        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            _queuedDirection = Vector2.up;
        else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            _queuedDirection = Vector2.down;
        else if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
            _queuedDirection = Vector2.left;
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
            _queuedDirection = Vector2.right;
        //Hacks
        else if (Keyboard.current.zKey.wasPressedThisFrame && GameSettings.instance.FreezeEnabled)
            FreezeHack();
        else if (Keyboard.current.cKey.wasPressedThisFrame && GameSettings.instance.CloneEnabled)
            CloneHack();
        else if (Keyboard.current.xKey.wasPressedThisFrame && GameSettings.instance.SpeedOverflowEnabled)
            SpeedOverflowHack();
        else if (Keyboard.current.vKey.wasPressedThisFrame && GameSettings.instance.FearOverrideEnabled)
            FearOverrideHack();
        else if (Keyboard.current.bKey.wasPressedThisFrame && GameSettings.instance.VisionHackEnabled)
            VisionHack();
    }

    private void TryQueuedDirection()
    {
        if (GameSettings.instance.WallPhaseEnabled)
        {
            if (_queuedDirection != Vector2.zero)
            {
                _currentDirection = _queuedDirection;
                _queuedDirection = Vector2.zero;
            }
            return;
        }

        if (_queuedDirection != Vector2.zero && CanMove(_queuedDirection))
        {
            _currentDirection = _queuedDirection;
            _queuedDirection = Vector2.zero;
        }
    }

    private void Move()
    {
        if (GameSettings.instance.WallPhaseEnabled)
        {
            if (_currentDirection != Vector2.zero)
            {
                float finalSpeed = moveSpeed;
                if (_isPacman)
                {
                    finalSpeed += GameSettings.instance.pacmanSpeed;
                }
                else
                {
                    finalSpeed += GameSettings.instance.ghostSpeed;
                }
            
                Vector2 translation = finalSpeed * Time.fixedDeltaTime * _currentDirection;
                _rb.MovePosition(_rb.position + translation);
            }
            return;
        }
        
        if (_currentDirection != Vector2.zero && CanMove(_currentDirection))
        {
            float finalSpeed = moveSpeed;
            if (_isPacman)
            {
                finalSpeed += GameSettings.instance.pacmanSpeed;
            }
            else
            {
                finalSpeed += GameSettings.instance.ghostSpeed;
            }
            
            Vector2 translation = finalSpeed * Time.fixedDeltaTime * _currentDirection;
            _rb.MovePosition(_rb.position + translation);
        }
    }

    private bool CanMove(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxCastSize, 0f, direction, raycastDistance, wallLayer);
        return hit.collider == null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, _currentDirection * raycastDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, _queuedDirection * raycastDistance);
    }
    
    private IEnumerator StartTimer(float duration, Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();
    }
    
    private IEnumerator StartStepTimer(float duration, Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(1f);
            onComplete?.Invoke();
            elapsed += 1f;
        }
    }
    
    private IEnumerator AnimateSliders(Slider slider, float duration)
    {
        if (duration <= 0)
        {
            slider.value = 0f;
            yield break;
        }

        float elapsed = 0f;
        float initialValue = slider.value;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / (float) duration);
            slider.value = Mathf.Lerp(initialValue, 0f, t);

            yield return null;
            elapsed += Time.deltaTime;
        }

        // Ensure slider ends at 0
        slider.value = 0f;
    }

    private void FreezeHack()
    {
        if (freezeHacksLeft <= 0 && freezeCoroutine == null) return;
            
        float duration = 10, sliderMaxVal = 0f;
        Slider slider = null;
        TMP_Text text = null;
            
        foreach (var e in GameSettings.instance.freezeHackUIElements)
        {
            if (e == null) continue;
            if (slider == null)
                slider = e.GetComponent<Slider>();
            if (text == null)
                text = e.GetComponent<TMP_Text>();

            if (slider != null)
                sliderMaxVal = slider.maxValue;
        }
        
        List<MonoBehaviour> blinkyScriptsEnabled = new();
        List<MonoBehaviour> inkyScriptsEnabled = new();
        List<MonoBehaviour> clydeScriptsEnabled = new();
        List<MonoBehaviour> pinkyScriptsEnabled = new();

        GameObject blinky = GameObject.Find("Blinky(Clone)");
        GameObject inky = GameObject.Find("Inky(Clone)");
        GameObject clyde = GameObject.Find("Clyde(Clone)");
        GameObject pinky = GameObject.Find("Pinky(Clone)");

        void DisableAndTrack(GameObject ghost, List<MonoBehaviour> trackedList)
        {
            if (ghost == null) return;
            foreach (var comp in ghost.GetComponents<MonoBehaviour>())
            {
                if (comp is GhostHome) continue;

                if (comp.enabled)
                {
                    trackedList.Add(comp);
                    comp.enabled = false;
                }
            }
        }

        DisableAndTrack(blinky, blinkyScriptsEnabled);
        DisableAndTrack(inky, inkyScriptsEnabled);
        DisableAndTrack(pinky, pinkyScriptsEnabled);
        DisableAndTrack(clyde, clydeScriptsEnabled);
            
        freezeHacksLeft--;
        if (text != null)
            text.text = "Freeze: \n [Z] - " + freezeHacksLeft + "x";
            
        freezeCoroutine = StartCoroutine(StartTimer(duration, () =>
        {
            if (slider != null)
                slider.value = sliderMaxVal;

            foreach (var comp in blinkyScriptsEnabled) comp.enabled = true;
            foreach (var comp in pinkyScriptsEnabled) comp.enabled = true;
            foreach (var comp in clydeScriptsEnabled) comp.enabled = true;
            foreach (var comp in inkyScriptsEnabled) comp.enabled = true;
                
            freezeCoroutine = null;
        }));

        if (slider != null)
            StartCoroutine(AnimateSliders(slider, duration));
    }

    private void CloneHack()
    {
        if (cloneHacksLeft <= 0) return;
            
        float duration = 10, sliderMaxVal = 0f;
        Slider slider = null;
        TMP_Text text = null;
            
        foreach (var e in GameSettings.instance.cloneHackUIElements)
        {
            if (e == null) continue;
            if (slider == null)
                slider = e.GetComponent<Slider>();
            if (text == null)
                text = e.GetComponent<TMP_Text>();

            if (slider != null)
                sliderMaxVal = slider.maxValue;
        }
            
        var pacmanClone = Instantiate(GameManager.Instance.pacmanPrefab, GameManager.Instance.pacmanSpawnPoint.position, GameManager.Instance.pacmanSpawnPoint.rotation);
        pacmanClone.name = "PacMan_Clone_Hack";
        pacmanClone.AddComponent<PacManAI>();

        cloneHacksLeft--;
        if (text != null)
            text.text = "Clone: \n [C] - " + cloneHacksLeft + "x";
            
        StartCoroutine(StartTimer(duration, () =>
        {
            if (slider != null)
                slider.value = sliderMaxVal;
            Destroy(pacmanClone);
                
            cloneCoroutine = null;
        }));

        if (slider != null)
            StartCoroutine(AnimateSliders(slider, duration));
    }

    private void SpeedOverflowHack()
    {
        if (speedHacksLeft <= 0 || speedCoroutine != null) return;

        float duration = 3, sliderMaxVal = 0f, originalSpeed = moveSpeed;
        Slider slider = null;
        TMP_Text text = null;

        foreach (var e in GameSettings.instance.speedOverflowUIElements)
        {
            if (e == null) continue;
            if (slider == null)
                slider = e.GetComponent<Slider>();
            if (text == null)
                text = e.GetComponent<TMP_Text>();

            if (slider != null)
                sliderMaxVal = slider.maxValue;
        }

        moveSpeed = originalSpeed * 3;
        speedHacksLeft--;
        if (text != null)
            text.text = "Speed: \n [X] - " + speedHacksLeft + "x";

        speedCoroutine = StartCoroutine(StartTimer(duration, () =>
        {
            moveSpeed = originalSpeed;
            if (slider != null)
                slider.value = sliderMaxVal;
            speedCoroutine = null;
        }));

        if (slider != null)
            StartCoroutine(AnimateSliders(slider, duration));
    }
    
    private void FearOverrideHack()
    {
        if (fearHacksLeft <= 0) return;
        
        TMP_Text text = null;
        foreach (var e in GameSettings.instance.fearHackUIElements)
        {
            if (e == null) continue; ;
            if (text == null)
                text = e.GetComponent<TMP_Text>();
        }

        string characterName = GameSettings.instance.selectedCharacter + "(Clone)";
        GameObject character = GameObject.Find(characterName);
        if (character != null)
        {
            var ghostScript = character.GetComponent<Ghost>();
            ghostScript.frightened.Disable();
        }
        
        fearHacksLeft--;
        if (text != null)
            text.text = "Fear: \n [V] - " + fearHacksLeft + "x";
    }
    
    private void VisionHack()
    {
        if (visionHacksLeft <= 0 || visionCoroutine != null) return;

        float duration = 5f, sliderMaxVal = 0f;
        Slider slider = null;
        TMP_Text text = null;

        foreach (var e in GameSettings.instance.visionHackUIElements)
        {
            if (e == null) continue;
            if (slider == null) slider = e.GetComponent<Slider>();
            if (text == null) text = e.GetComponent<TMP_Text>();

            if (slider != null) sliderMaxVal = slider.maxValue;
        }

        //TODO: Add vision hack effect; reveal PacMan's AI planned path;
        
        visionHacksLeft--;
        if (text != null) text.text = "Vision: \n [B] - " + visionHacksLeft + "x";
        
        if (slider != null)
            StartCoroutine(AnimateSliders(slider, duration));
    }

}