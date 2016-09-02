﻿using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Settings.Master;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Navigation;

namespace PokemonGo_UWP.ViewModels
{
    public class PokedexPageViewModel : ViewModelBase
    {
        #region Lifecycle Handlers
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.Any())
            {
                PokemonFoundAndSeen = (ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>)state[nameof(PokemonFoundAndSeen)];
                SeenPokemons = (int)state[nameof(SeenPokemons)];
                CapturedPokemons = (int)state[nameof(CapturedPokemons)];
            }
            else
            {
                var pokedexItems = GameClient.PokedexInventory;
                var lastPokemonIdSeen = pokedexItems == null || pokedexItems.Count == 0 ? 0 : pokedexItems.Max(x => (int)x.PokemonId);
                if (lastPokemonIdSeen > 0)
                {
                    var listAllPokemon = Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>();
                    foreach (var item in listAllPokemon)
                    {
                        if ((int) item > lastPokemonIdSeen)
                        {
                            break;
                        }

                        switch (item)
                        {
                            case PokemonId.Missingno:
                            {
                                break;
                            }
                            default:
                            {
                                var pokedexEntry = pokedexItems.Where(x => x.PokemonId == item);
                                if (pokedexEntry.Count() == 1)
                                {
                                    PokemonFoundAndSeen.Add(new KeyValuePair<PokemonId, PokedexEntry>(item,
                                        pokedexEntry.ElementAt(0)));
                                }
                                else
                                {
                                    PokemonFoundAndSeen.Add(new KeyValuePair<PokemonId, PokedexEntry>(item, null));
                                }
                                break;
                            }
                        }
                    }

                    CapturedPokemons = pokedexItems.Count(x => x.TimesCaptured > 0);
                    SeenPokemons = pokedexItems.Count;
                }
                else
                {
                    CapturedPokemons = 0;
                    SeenPokemons = 0;
                }
            }

            if (parameter is PokemonId)
            {
                NavigationService.Navigate(typeof(PokedexDetailPage), (PokemonId)parameter);
            }

            NavigationService.FrameFacade.BackRequested += FrameFacade_BackRequested;
            return Task.CompletedTask;
        }

        private void FrameFacade_BackRequested(object sender, Template10.Common.HandledEventArgs e)
        {
            e.Handled = true;
            CloseCommand.Execute();
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            if (suspending)
            {
                pageState[nameof(PokemonFoundAndSeen)] = PokemonFoundAndSeen;
                pageState[nameof(SeenPokemons)] = SeenPokemons;
                pageState[nameof(CapturedPokemons)] = CapturedPokemons;
            }
            else
            {
                pageState?.Clear();
                PokemonFoundAndSeen?.Clear();
                EeveeEvolutions?.Clear();
                PokemonEvolutions?.Clear();
            }
            NavigationService.FrameFacade.BackRequested -= FrameFacade_BackRequested;
            return Task.CompletedTask;
        }
        #endregion

        #region Variables
        public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonFoundAndSeen { get; private set; } = new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();
        private int _captured, _seen;
        public int CapturedPokemons { get { return _captured; } set { Set(ref _captured, value); } }
        public int SeenPokemons { get { return _seen; } set { Set(ref _seen, value); } }
        private PokemonSettings _pokemonDetails;
        public PokemonSettings PokemonDetails { get { return _pokemonDetails; } set { Set(ref _pokemonDetails, value); } }
        public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonEvolutions { get; } = new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();
        public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> EeveeEvolutions { get; } = new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();
        #endregion

        private DelegateCommand<KeyValuePair<PokemonId, PokedexEntry>> _openPokedexEntry;
        public DelegateCommand<KeyValuePair<PokemonId, PokedexEntry>> OpenPokedexEntry =>
            _openPokedexEntry ??
            (_openPokedexEntry = new DelegateCommand<KeyValuePair<PokemonId, PokedexEntry>>(
                (x) =>
                    {
                        NavigationService.Navigate(typeof(PokedexDetailPage), x.Key);
                    },
                    (x) => true)
            );

        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand
            =>
            _closeCommand ??
            (_closeCommand = new DelegateCommand(() =>
                {
                    NavigationService.GoBack();
                }, () => true)
            );
    }
}
