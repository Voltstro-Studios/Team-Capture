// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.IO;
using Steamworks;
using Team_Capture.Core;
using Team_Capture.Core.UserAccount;
using Team_Capture.Helper;
using Team_Capture.Logging;

namespace Team_Capture.Integrations.Steamworks
{
	/// <summary>
	///		Handles connecting to Steam
	/// </summary>
    internal class SteamManager : SingletonMonoBehaviour<SteamManager>
	{
		/// <summary>
	    ///     Where to load the settings from
	    /// </summary>
	    public string settingsLocation = "/Resources/Integrations/Steam.json";

	    private SteamManagerSettings settings;

	    private AuthTicket authTicket;

	    protected override void SingletonAwakened()
	    {
	    }

	    protected override void SingletonStarted()
	    {
		    LoadSettings();
			Initialize();
	    }

	    protected override void SingletonDestroyed()
	    {
		    SteamClient.Shutdown();
	    }

	    private void Update()
	    {
		    SteamClient.RunCallbacks();
	    }

	    private void LoadSettings()
	    {
		    if (string.IsNullOrWhiteSpace(settingsLocation))
			    return;

		    settings = ObjectSerializer.LoadJson<SteamManagerSettings>(
			    Path.GetDirectoryName($"{Game.GetGameExecutePath()}{settingsLocation}"),
			    $"/{Path.GetFileNameWithoutExtension(settingsLocation)}");
	    }

	    private void Initialize()
	    {
		    try
		    {
			    SteamClient.Init(settings.appId);
		    }
		    catch (Exception ex)
		    {
				Logger.Error("Something went wrong while starting the Steam integration! {ExceptionMessage}", ex.Message);
				Destroy(gameObject);
				return;
		    }

		    if (!SteamClient.IsLoggedOn)
		    {
				Logger.Error("Could not connect to Steam!");
				Destroy(gameObject);
				return;
		    }

			User.AddAccount(new Account
			{
				AccountProvider = AccountProvider.Steam,
				AccountName = SteamClient.Name,
				AccountId = SteamClient.SteamId.Value
			});

			Logger.Info("Logged into Steam account {AccountName} with an ID of {AccountID}", SteamClient.Name, SteamClient.SteamId.Value);
	    }
    }
}