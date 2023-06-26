using AiC_PCD.Model;
using Plugin.Maui.Audio;

namespace AiC_PCD;

public partial class MainPage : ContentPage
{
	IAudioPlayer castStart, castAlter, castConfirm, casting, castingExt1, castingExt2, castDone, castHold1, castHold2,
				castArrowCharge, castArrowRelease, castBallRelease, castBombRelease, castBombExplode, castCancel;
	public MainPage()
	{
		InitializeComponent();
		Task.Run(() =>
		{
			while (true)
			{   //The select rectangle animation.
				_ = Select_Rect.ScaleTo(0.85, easing: Easing.SinIn);
				Thread.Sleep(250);
				_ = Select_Rect.ScaleTo(1, easing: Easing.SinOut);
				Thread.Sleep(250);
			}
		});
	}

	const ushort arrowCastTime = 500, ballCastTime = 1234, ltngCastTime = 800, bombCastTime = 500;

	Magic? selected = null;
	Magic? magic2Cast = null;
	bool isHolding = false, isCasting = false, isMagicReleasing = false;
	private async void Cast_Arrow_Tapped(object sender, TappedEventArgs e)
	{
		#region Init SFX
		castStart ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\cast_select.wav"));
		castAlter ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\cast_alter.wav"));
		#endregion
		if (isCasting || isMagicReleasing) return;
		if (magic2Cast is not null)
		{   //Cast the magic.
			isHolding = false;
			CastMagic((Magic)magic2Cast);
			magic2Cast = null;
		}
		else if (selected is null)
		{   //Select the magic.
			selected = Magic.Arrow;
			castStart!.Stop();
			castStart!.Play();
			//2*120cos(45)≈169.7
			_ = Cast_Ball.TranslateTo(169, 0, 300, Easing.CubicOut);
			_ = Cast_Ltng.TranslateTo(0, -169, 300, Easing.CubicOut);
			_ = Cast_Bomb.TranslateTo(0, 169, 300, Easing.CubicOut);
			_ = Cast_Ball.FadeTo(100, 1000, Easing.SinOut);
			_ = Cast_Ltng.FadeTo(100, 1000, Easing.SinOut);
			_ = Cast_Bomb.FadeTo(100, 1000, Easing.SinOut);
			_ = Select_Rect.IsVisible = true;
		}
		else if (selected is Magic.Arrow)   //Cast WhiteArrow magic.
			ConfirmCast(Magic.Arrow, arrowCastTime);
		else
			SelectMagic(Magic.Arrow, arrowCastTime);
	}
	private void Cast_Ball_Tapped(object sender, TappedEventArgs e)
	{
		SelectMagic(Magic.Ball, ballCastTime);
	}
	private void Cast_Ltng_Tapped(object sender, TappedEventArgs e)
	{
		SelectMagic(Magic.Lightning, ltngCastTime);
	}
	private void Cast_Bomb_Tapped(object sender, TappedEventArgs e)
	{
		SelectMagic(Magic.Bomb, bombCastTime);
	}
	private async void Cast_Arrow_Swipe_Left(object sender, SwipedEventArgs e)
	{
		if (isHolding && !isCasting && !isMagicReleasing)
		{
			castCancel ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\magic_chanted_hold_pr.wav"));
			ResetTransform();
			magic2Cast = null;
			isHolding = false;
			castCancel.Play();
			castHold1.Stop();
			castHold2.Stop();
		}
	}
	private void BG_Tapped(object sender, TappedEventArgs e)
	{
		ResetTransform();
		if (selected is not null)
			selected = null;
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
		_ = Select_Rect.TranslateTo(0, 0);
		_ = Select_Rect.IsVisible = false;
		Cast_Arrow.Stroke = Colors.White;
	}
	private void SelectMagic(Magic magic, ushort castTime)
	{
		if (selected == magic)
			ConfirmCast(magic, castTime);
		else
		{
			_ = (selected = magic) switch
			{
				Magic.Arrow => Select_Rect.TranslateTo(0, 0, 200, Easing.CubicOut),
				Magic.Ball => Select_Rect.TranslateTo(169, 0, 200, Easing.CubicOut),
				Magic.Lightning => Select_Rect.TranslateTo(0, -169, 200, Easing.CubicOut),
				Magic.Bomb => Select_Rect.TranslateTo(0, 169, 200, Easing.CubicOut),
				_ => throw new NotImplementedException(),
			};
			castAlter!.Stop();
			castAlter!.Play();
		}
	}
	Animation white2red, white2green, white2blue;
	private async void ConfirmCast(Magic magic, ushort castTime)
	{
		#region Initialize variables
		castConfirm ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\cast_confirm.wav"));
		casting ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\magic_casting_pr.wav"));
		castDone ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\cast_targetting.wav"));
		white2red ??= new Animation(v => Cast_Arrow.Stroke = Color.FromRgb(255, 1 - v, 1 - v), 0, 1);
		white2green ??= new Animation(v => Cast_Arrow.Stroke = Color.FromRgb(1 - v, 255, 1 - v), 0, 1);
		white2blue ??= new Animation(v => Cast_Arrow.Stroke = Color.FromRgb(1 - v, 1 - v, 255), 0, 1);
		#endregion
		switch (magic)
		{
			case Magic.Ball:
				white2red.Commit(Cast_Arrow, "ColorAnimation", length: ballCastTime, easing: Easing.BounceOut);
				break;
			case Magic.Lightning:
				white2green.Commit(Cast_Arrow, "ColorAnimation", length: ltngCastTime, easing: Easing.BounceOut);
				break;
			case Magic.Bomb:
				white2blue.Commit(Cast_Arrow, "ColorAnimation", length: bombCastTime, easing: Easing.BounceOut);
				break;
		}
		ResetTransform();
		selected = null;
		castConfirm.Stop(); castConfirm.Play();
		casting.Play();
		isCasting = true;
		Task.Run(() =>
		{
			Thread.Sleep(1200);
			if (isCasting) LoopCasting();
		});
		await Task.Run(() => Thread.Sleep(castTime));
		isCasting = false; isHolding = true;
		castDone.Play();
		casting.Stop();
		castingExt1?.Stop(); castingExt2?.Stop();
		magic2Cast = magic;
		LoopCastHold();
	}
	private async void CastMagic(Magic magic)
	{
		switch (magic)
		{
			case Magic.Arrow:
				castArrowCharge ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\mg_whitearrow_appear.wav"));
				castArrowRelease ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\mg_whitearrow_shot0.wav"));
				castArrowRelease!.PlaybackEnded += (sender, e) => isMagicReleasing = false;
				isMagicReleasing = true;
				castArrowCharge!.Play();
				Thread.Sleep(600);  //Wait for the arrow to charge.
				castArrowRelease!.Play();
				break;
			case Magic.Ball:
				castBallRelease ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\mg_fireball.wav"));
				castBallRelease!.PlaybackEnded += (sender, e) => isMagicReleasing = false;
				isMagicReleasing = true;
				castBallRelease!.Play();
				break;
			case Magic.Lightning:
			case Magic.Bomb:
				castBombRelease ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\dropbomb_init.wav"));
				castBombRelease!.PlaybackEnded += (sender, e) => isMagicReleasing = false;
				isMagicReleasing = true;
				castBombRelease!.Play();
				break;
		}
		Cast_Arrow.Stroke = Colors.White;
	}

	private async void LoopCastHold()
	{
		castHold1 ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\magic_hold_pr.wav"));
		castHold2 ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\magic_hold_pr.wav"));
		castHold1.Volume = 0.66;
		castHold2.Volume = 0.66;
		Task.Run(() =>
		{
			while (isHolding)
			{
				castHold1.Play();
				Thread.Sleep(300);
				castHold2.Play();
				Thread.Sleep(300);
			}
		});
	}
	private async void LoopCasting()
	{
		castingExt1 ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\swing_wheel.wav"));
		castingExt2 ??= AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("SFX\\swing_wheel.wav"));
		Task.Run(() =>
		{
			while (isCasting)
			{
				castingExt1.Play();
				Thread.Sleep(200);
				if (!isCasting) break;
				castingExt2.Play();
				Thread.Sleep(200);
			}
			castingExt1.Stop();
			castingExt2.Stop();
		});
	}
	#endregion
}

