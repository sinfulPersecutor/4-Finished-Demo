using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace RPG.Characters
{
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] float maxHealthPoints = 100f;
        [SerializeField] Image healthBar;
        [SerializeField] AudioClip[] damageSounds;
        [SerializeField] AudioClip[] deathSounds;
        [SerializeField] float deathVanishSeconds = 2.0f;

        const string DEATH_TRIGGER = "Death";

        float currentHealthPoints; 
        Animator animator;
        AudioSource audioSource;
        Character character;

        public float healthAsPercentage { get { return currentHealthPoints / maxHealthPoints; } }

        void Start()
        {
            animator = GetComponent<Animator>();
            character = GetComponent<Character>();
            audioSource = character.GetAudioSource();

            currentHealthPoints = maxHealthPoints;
        }

        void Update()
        {
            UpdateHealthBar();
        }

        void UpdateHealthBar()
        {
            if (healthBar) // Enemies may not have health bars to update
            {
                healthBar.fillAmount = healthAsPercentage;
            }
        }

        public void TakeDamage(float damage)
        {
            bool characterDies = (currentHealthPoints - damage <= 0);
            currentHealthPoints = Mathf.Clamp(currentHealthPoints - damage, 0f, maxHealthPoints);
            var clip = damageSounds[UnityEngine.Random.Range(0, damageSounds.Length)];
            audioSource.PlayOneShot(clip);
            if (characterDies)
            {
                StartCoroutine(KillCharacter());
            }
        }

        public void Heal(float points)
        {
            currentHealthPoints = Mathf.Clamp(currentHealthPoints + points, 0f, maxHealthPoints);
        }

        IEnumerator KillCharacter()
        {
            character.Kill();
            animator.SetTrigger(DEATH_TRIGGER);

            var clip = deathSounds[UnityEngine.Random.Range(0, deathSounds.Length)];
            PlayDeathSound(clip);
            yield return new WaitForSecondsRealtime(audioSource.clip.length); // TODO discuss with Sam

            var playerComponent = GetComponent<PlayerControl>();
            bool characterIsPlayer = playerComponent && playerComponent.isActiveAndEnabled;
            if (characterIsPlayer)
            {
                SceneManager.LoadScene(0);
            }
            else // assume is enemy for now, reconsider on other NPCs
            {
                DestroyObject(gameObject, deathVanishSeconds);
            }
        }
        
        void PlayDeathSound(AudioClip clip)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(clip);
        }
    }
}