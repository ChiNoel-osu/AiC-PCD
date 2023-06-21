using AiC_PCD.Model;
using Plugin.Maui.Audio;

namespace AiC_PCD;

public partial class MainPage : ContentPage
{
	IAudioPlayer castStart, castAlter, castConfirm, casting, castDone, castHold1, castHold2;
	public MainPage()
	{
		InitializeComponent();
	}

	Magic? selected = null;
	bool isHolding = false;
	private async void Cast_Arrow_Tapped(object sender, TappedEventArgs e)
	{
		#region Init SFX
		castStart ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\cast_select.wav"));
		castAlter ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\cast_alter.wav"));
		#endregion
		if (selected is null)
		{
			selected = Magic.Arrow;
			castStart.Stop();
			castStart.Play();
			//2*120cos(45)≈169.7
			_ = Cast_Ball.TranslateTo(169, 0, 300, Easing.CubicOut);
			_ = Cast_Ltng.TranslateTo(0, -169, 300, Easing.CubicOut);
			_ = Cast_Bomb.TranslateTo(0, 169, 300, Easing.CubicOut);
			_ = Cast_Ball.FadeTo(100, 1000, Easing.SinOut);
			_ = Cast_Ltng.FadeTo(100, 1000, Easing.SinOut);
			_ = Cast_Bomb.FadeTo(100, 1000, Easing.SinOut);
		}
		else if (selected is Magic.Arrow)
			ConfirmCast(500);
		else
		{
			selected = Magic.Arrow;
			castAlter.Stop();
			castAlter.Play();
		}
	}
	private void Cast_Ball_Tapped(object sender, TappedEventArgs e)
	{
		SelectMagic(Magic.Ball);
	}
	private void Cast_Ltng_Tapped(object sender, TappedEventArgs e)
	{
		SelectMagic(Magic.Lightning);
	}
	private void Cast_Bomb_Tapped(object sender, TappedEventArgs e)
	{
		SelectMagic(Magic.Bomb);
	}

	private void BG_Tapped(object sender, TappedEventArgs e)
	{
		ResetTransform();
		if (selected is not null)
		{
			selected = null;
		}
	}

	#region Extracted Methods
	private void ResetTransform()
	{
		_ = Cast_Ball.TranslateTo(0, 0, 100);
		_ = Cast_Ltng.TranslateTo(0, 0, 100);
		_ = Cast_Bomb.TranslateTo(0, 0, 100);
		_ = Cast_Ball.FadeTo(0, 100);
		_ = Cast_Ltng.FadeTo(0, 100);
		_ = Cast_Bomb.FadeTo(0, 100);
	}
	private void SelectMagic(Magic magic2Cast)
	{
		if (selected == magic2Cast)
			ConfirmCast(1000);
		else
		{
			selected = magic2Cast;
			castAlter.Stop();
			castAlter.Play();
		}
	}
	private async void ConfirmCast(ushort castTime)
	{
		castConfirm ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\cast_confirm.wav"));
		casting ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\magic_casting_pr.wav"));
		castDone ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\cast_targetting.wav"));
		ResetTransform();
		selected = null;
		castConfirm.Stop();
		castConfirm.Play();
		casting.Play();
		await Task.Run(() => Thread.Sleep(castTime));
 		castDone.Play();
		casting.Stop();
		LoopCastHold();
		isHolding = true;
		isHolding = false;
	}
	private async void LoopCastHold()
	{
		castHold1 ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\magic_hold_pr.wav"));
		castHold2 ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\magic_hold_pr.wav"));
		castHold1.Volume = 0.5;
		castHold2.Volume = 0.5;
		Task.Run(() =>
		{
			do
			{
				castHold1.Play();
				Thread.Sleep(300);
				castHold2.Play();
				Thread.Sleep(300);
			}
			while (isHolding);
		});
	}
	#endregion
}

