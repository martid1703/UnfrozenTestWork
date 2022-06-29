using System;
using System.Collections;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;

namespace UnfrozenTestWork
{
    [RequireComponent(typeof(UnitAnimatorController))]
    [RequireComponent(typeof(AudioController))]
    public class UnitController : MonoBehaviour, IUnitController
    {
        [SerializeField]
        private GameObject _popupPrefab;

        [Space]
        [SerializeField]
        private SkeletonAnimation _skeletonAnimation;

        [SpineEvent(dataField: "skeletonAnimation", fallbackToTextField: true)]
        public string footstepEvent;

        [SpineEvent(dataField: "skeletonAnimation", fallbackToTextField: true)]
        public string attackEvent;

        [SpineEvent(dataField: "skeletonAnimation", fallbackToTextField: true)]
        public string takeDamageEvent;

        private IUnitAnimatorController _animatorController;
        private HealthBarController _healthBarController;
        private InitiativeBarController _initiativeBarController;
        private AudioController _audioController;
        private float _battleSpeed;
        private Spine.EventData _footstepEventData;
        private Spine.EventData _attackEventData;
        private Spine.EventData _takeDamageEventData;
        private TextMeshPro _unitInfo;


        private void Awake()
        {
            _animatorController = GetComponent<IUnitAnimatorController>();
            _healthBarController = GetComponentInChildren<HealthBarController>();
            _initiativeBarController = GetComponentInChildren<InitiativeBarController>();
            _audioController = GetComponentInChildren<AudioController>();
            _unitInfo = transform.GetComponentInChildren<TextMeshPro>();
        }

        private void Start()
        {
            SetAnimationEvents();
        }

        private void SetAnimationEvents()
        {
            _footstepEventData = _skeletonAnimation.Skeleton.Data.FindEvent(footstepEvent);
            _attackEventData = _skeletonAnimation.Skeleton.Data.FindEvent(attackEvent);
            _takeDamageEventData = _skeletonAnimation.Skeleton.Data.FindEvent(takeDamageEvent);
            _skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
        }

        public void Initialize(UnitData unitData)
        {
            _healthBarController.Initialize(unitData.Health);
            _initiativeBarController.Initialize(unitData.Initiative);
        }

        // a.t.m. not all given spine animations have required attack events, threfore some sounds are played manually
        private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
        {
            if (_footstepEventData == e.Data)
            {
                _audioController.PlayStep();
            }
            if (_attackEventData == e.Data)
            {
                _audioController.PlayAttack();
            }
            if (_takeDamageEventData == e.Data)
            {
                _audioController.PlayDamage();
            }
        }

        public void SetBattleSpeed(float speed)
        {
            _animatorController.SetAnimationSpeed(speed);
            _battleSpeed = speed;
        }

        public void Idle()
        {
            _animatorController.SetCharacterState(PlayerAnimationState.Idle, true);
        }

        public IEnumerator Attack(Action onComplete = null)
        {
            float animationDuration = _animatorController.SetCharacterState(PlayerAnimationState.Attack, false);
            _audioController.PlayAttack();
            yield return new WaitForSecondsRealtime(animationDuration);
        }

        public IEnumerator TakeDamage(float damage, UnitData unitData, Action onComplete = null)
        {
            float animationDuration = _animatorController.SetCharacterState(PlayerAnimationState.TakeDamage, false);

            _audioController.PlayDamage();
            StartCoroutine(ShowDamagePopup(damage, unitData.Health, animationDuration));

            _healthBarController.SetReduceHPSpeed(BattleSpeedConverter.GetHPReduceSpeed(_battleSpeed));
            StartCoroutine(_healthBarController.TakeDamage(damage));

            _unitInfo.text = ToString();

            yield return new WaitForSecondsRealtime(animationDuration);
        }

        private IEnumerator ShowDamagePopup(float damage, float health, float destroyTime)
        {
            var popup = Instantiate(_popupPrefab, transform.position, Quaternion.identity);
            if (IsCriticalDamage(damage, health))
            {
                Debug.Log($"Taking CRITICAL damage: {damage}. Current HP: {health}. Health remaining: {health - damage}.");
                yield return popup.GetComponent<DamagePopup>().CriticalPopup(damage, transform.position, destroyTime);
            }
            else
            {
                Debug.Log($"Taking damage: {damage}. Current HP: {health}. Health remaining: {health - damage}");
                yield return popup.GetComponent<DamagePopup>().RegularPopup(damage, transform.position, destroyTime);
            }
            Destroy(popup);
        }

        private bool IsCriticalDamage(float damage, float health)
        {
            if (damage > health / 2)
            {
                return true;
            }
            return false;
        }

        public IEnumerator Run()
        {
            float duration = _animatorController.SetCharacterState(PlayerAnimationState.Run, true);
            yield return null;
        }

        public void Die()
        {
            float duration = _animatorController.SetCharacterState(PlayerAnimationState.Die, false);
        }
    }
}
