using System;
using System.Collections.Generic;
using System.Linq;
using CreatureControllers;
using UnityEngine;
using Zenject;

namespace Managers
{
    public interface ITutorialManager
    {
        void StartTutorial();
        event Action<TutorialStep> TutorialChanged;
        TutorialStep CurrentStep { get; }
        
        public class TutorialStep
        {
            public string Description { get; }
            public Func<bool> Condition { get; }

            public TutorialStep(string description, Func<bool> condition)
            {
                Description = description;
                Condition = condition;
            }
        }
    }

    public class TutorialManager : MonoBehaviour, ITutorialManager
    {
        public event Action<ITutorialManager.TutorialStep> TutorialChanged;
        public ITutorialManager.TutorialStep CurrentStep { get; private set; }

        private ITutorialManager.TutorialStep[] _tutorialSteps;

        [Inject] private ISelectionManager _selectionManager;
        [Inject] private ICreatureManager _creatureManager;
        [Inject] private ISoundPlayer _soundPlayer;

        [SerializeField] private AudioClip tutorialStepCompletedSound;

      private void Awake()
        {
            _tutorialSteps = new[]
            {
                new ITutorialManager.TutorialStep(
                    "Tutorial.Welcome",
                    () => IsPressingSkipButtons() || GetSelectedCreatures().Any()),
                new ITutorialManager.TutorialStep(
                    "Tutorial.SelectUnit",
                    () => GetSelectedCreatures().Any()),
                new ITutorialManager.TutorialStep(
                    "Tutorial.MoveCommand",
                    () => GetPlayerCreaturesControllers().Any(x => x.MoveCommandTarget != null)),
                new ITutorialManager.TutorialStep(
                    "Tutorial.Explore",
                    () => GetSelectedCreatures().All(x => x.transform.position.y > 60.5f)),
                new ITutorialManager.TutorialStep(
                    "Tutorial.SearchCabinet",
                    () => GetPlayerCreatures().Any(x => x.Inventory.Items.Count >= 1)),
                new ITutorialManager.TutorialStep(
                    "Tutorial.OpenInventory",
                    () => Input.GetKeyDown(KeyCode.I) || (Input.GetKeyDown(KeyCode.F) && GetSelectedCreatures().Any())),
                new ITutorialManager.TutorialStep(
                    "Tutorial.EquipRifle",
                    () => GetPlayerCreatures().Any(x => x.Weapon != null)),
                new ITutorialManager.TutorialStep(
                    "Tutorial.KillZombieMove",
                    () => GetSelectedCreatures().Any(x =>
                        GameObject.Find("FirstZombie") == null ||
                        Vector2.Distance(x.transform.position,
                                         GameObject.Find("FirstZombie").transform.position) < 6f)),
                new ITutorialManager.TutorialStep(
                    "Tutorial.KillZombie",
                    () => GameObject.Find("FirstZombie") == null),
                new ITutorialManager.TutorialStep(
                    "Tutorial.ProceedDown",
                    () => GetSelectedCreatures().All(x =>
                        x.transform.position.y < 38.0f &&
                        x.transform.position.x > 60.37561f)),
                new ITutorialManager.TutorialStep(
                    "Tutorial.CryoPod",
                    () => GetPlayerCreatures().Count() == 2),
                new ITutorialManager.TutorialStep(
                    "Tutorial.NewFriend",
                    () => GameObject.Find("Zombie") == null),
                new ITutorialManager.TutorialStep(
                    "Tutorial.Extract",
                    () => GetPlayerCreatures().All(x => !x.gameObject.activeInHierarchy))
            };

            gameObject.SetActive(false);
        }

        private bool IsPressingSkipButtons()
        {
            return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space) ||
                   Input.GetKeyDown(KeyCode.Return);
        }

        private IEnumerable<Creature> GetSelectedCreatures()
        {
            return _selectionManager.SelectedCreatures;
        }

        private IEnumerable<Creature> GetPlayerCreatures()
        {
            return _creatureManager.PlayerCreatures;
        }

        private IEnumerable<UnitController> GetPlayerCreaturesControllers()
        {
            return _creatureManager.PlayerCreatures
                .Select(x => x.Controller)
                .OfType<UnitController>();
        }

        private void Initialize()
        {
        }

        private void Update()
        {
            if (CurrentStep == null)
                return;

            if (CurrentStep.Condition())
            {
                CurrentStep = GetNextStep();
                TutorialChanged?.Invoke(CurrentStep);

                if (tutorialStepCompletedSound != null)
                    _soundPlayer.PlaySoundGlobal(tutorialStepCompletedSound, SoundType.UI);
            }
        }

        private ITutorialManager.TutorialStep GetNextStep()
        {
            int currentIndex = Array.IndexOf(_tutorialSteps, CurrentStep);
            if (currentIndex < _tutorialSteps.Length - 1)
            {
                return _tutorialSteps[currentIndex + 1];
            }

            // If no more steps, deactivate the tutorial
            gameObject.SetActive(false);
            return null;
        }

        public void StartTutorial()
        {
            Initialize();

            gameObject.SetActive(true);

            CurrentStep = _tutorialSteps[0];
            TutorialChanged?.Invoke(CurrentStep);
        }
    }
}