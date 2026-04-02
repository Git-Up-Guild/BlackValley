using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace BlackValley.World;

/// <summary>
/// 管理农场中的幽灵遭遇点。
/// 当前先做固定刷新的世界实体，后续再接近距离触发战斗。
/// </summary>
internal sealed class FarmEncounterManager
{
    private const string FarmLocationName = "Farm";
    private const string VanillaGhostTexturePath = "Characters/Monsters/Ghost";
    private const string VanillaGhostSpriteName = "Characters\\Monsters\\Ghost";
    private const int SaveLoadGraceTicks = 30;

    private static readonly Vector2[] GhostAnchorPixels =
    {
        new(4070f, 1385f),
        new(4316f, 1435f),
        new(3940f, 1637f),
        new(4513f, 1701f)
    };

    private readonly IModHelper _helper;
    private readonly IMonitor _monitor;
    private readonly Texture2D _fallbackGhostTexture;
    private readonly List<FarmEncounterGhost> _ghosts = new();

    private Texture2D? _vanillaGhostTexture;
    private bool _loggedVanillaFallback;
    private bool _playerWasInEncounterRange;
    private int _saveLoadGraceTicksRemaining;
    private FarmEncounterGhost? _activeEncounterGhost;

    public FarmEncounterManager(IModHelper helper, IMonitor monitor, BattleAssets battleAssets)
    {
        _helper = helper;
        _monitor = monitor;
        _fallbackGhostTexture = battleAssets.GhostTexture;
    }

    public void InitializeForSave()
    {
        _ghosts.Clear();
        _activeEncounterGhost = null;
        _playerWasInEncounterRange = false;
        _saveLoadGraceTicksRemaining = SaveLoadGraceTicks;

        for (int index = 0; index < GhostAnchorPixels.Length; index++)
        {
            _ghosts.Add(new FarmEncounterGhost(FarmLocationName, GhostAnchorPixels[index], index));
        }

        _vanillaGhostTexture = null;
        TryLoadVanillaGhostTexture();
        _monitor.Log($"Initialized {_ghosts.Count} farm encounter ghosts.", LogLevel.Info);
    }

    public void Clear()
    {
        _ghosts.Clear();
        _activeEncounterGhost = null;
        _playerWasInEncounterRange = false;
        _saveLoadGraceTicksRemaining = 0;
    }

    public void Update()
    {
        if (!Context.IsWorldReady)
        {
            return;
        }

        foreach (FarmEncounterGhost ghost in _ghosts)
        {
            ghost.Update();
        }

        if (_saveLoadGraceTicksRemaining > 0)
        {
            _saveLoadGraceTicksRemaining--;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Context.IsWorldReady || Game1.currentLocation == null || _ghosts.Count == 0)
        {
            return;
        }

        Texture2D ghostTexture = GetGhostTexture(out bool isUsingFallbackTexture);

        foreach (FarmEncounterGhost ghost in _ghosts)
        {
            if (!ghost.IsInLocation(Game1.currentLocation))
            {
                continue;
            }

            ghost.Draw(spriteBatch, ghostTexture, isUsingFallbackTexture);
        }
    }

    public bool ShouldTriggerEncounter(bool canTrigger)
    {
        if (!Context.IsWorldReady || Game1.player == null || Game1.currentLocation == null || _ghosts.Count == 0)
        {
            _playerWasInEncounterRange = false;
            return false;
        }

        Vector2 playerStandingPixel = Game1.player.getStandingPosition();
        bool isPlayerInEncounterRange = false;
        FarmEncounterGhost? triggeredGhost = null;

        foreach (FarmEncounterGhost ghost in _ghosts)
        {
            if (!ghost.IsPlayerInTriggerRange(Game1.currentLocation, playerStandingPixel))
            {
                continue;
            }

            isPlayerInEncounterRange = true;
            triggeredGhost = ghost;
            break;
        }

        bool shouldTrigger = canTrigger
            && _saveLoadGraceTicksRemaining <= 0
            && isPlayerInEncounterRange
            && !_playerWasInEncounterRange;

        if (shouldTrigger)
        {
            _activeEncounterGhost = triggeredGhost;
        }

        _playerWasInEncounterRange = isPlayerInEncounterRange;
        return shouldTrigger;
    }

    public void ResolveActiveEncounterVictory()
    {
        if (!Context.IsWorldReady || Game1.currentLocation == null || _activeEncounterGhost == null)
        {
            _activeEncounterGhost = null;
            return;
        }

        Vector2 deathEffectPosition = _activeEncounterGhost.GetDeathEffectPosition();
        Game1.currentLocation.localSound("ghost");
        Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(
            VanillaGhostSpriteName,
            new Rectangle(0, 96, 16, 24),
            100f,
            4,
            0,
            deathEffectPosition,
            flicker: false,
            flipped: false,
            0.9f,
            0.001f,
            Color.White,
            4f,
            0.01f,
            0f,
            MathF.PI / 64f));

        _ghosts.RemoveAll(ghost => ghost.MatchesLocationAndAnchor(Game1.currentLocation.NameOrUniqueName, deathEffectPosition + new Vector2(32f, 64f)));
        _activeEncounterGhost = null;
        _playerWasInEncounterRange = false;
    }

    public void ResolveActiveEncounterDefeat()
    {
        _activeEncounterGhost = null;
        _playerWasInEncounterRange = false;
    }

    private Texture2D GetGhostTexture(out bool isUsingFallbackTexture)
    {
        TryLoadVanillaGhostTexture();

        if (_vanillaGhostTexture != null)
        {
            isUsingFallbackTexture = false;
            return _vanillaGhostTexture;
        }

        isUsingFallbackTexture = true;
        return _fallbackGhostTexture;
    }

    private void TryLoadVanillaGhostTexture()
    {
        if (_vanillaGhostTexture != null)
        {
            return;
        }

        try
        {
            _vanillaGhostTexture = _helper.GameContent.Load<Texture2D>(VanillaGhostTexturePath);
        }
        catch (Exception exception)
        {
            if (_loggedVanillaFallback)
            {
                return;
            }

            _loggedVanillaFallback = true;
            _monitor.Log(
                $"Failed to load vanilla ghost texture from '{VanillaGhostTexturePath}', using fallback asset instead: {exception.Message}",
                LogLevel.Warn);
        }
    }
}
