using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Compats
{
    // 将此文件复制到你的模组中，把上面的命名空间改成你自己的，并阅读以下注释
    /// <summary>
    /// 用于 mod.Call 包装器的核心文件。
    /// </summary>
    internal class ClickerCompat : ModSystem
    {
        //基本信息 - 请先阅读！
        //-----------------------
        //https://github.com/SamsonAllen13/ClickerClassExampleMod/wiki
        //
        //此文件与最新版 Clicker Class 保持同步。建议你不要编辑此文件，当模组更新时，重新复制替换此文件即可。
        //如果 Clicker Class 更新了而你的模组没有更新，不会出现任何问题，是否进一步更新完全由你决定。
        //-----------------------

        //这是模组调用所使用的 API 版本。
        //如果 Clicker Class 更新了，旧版本的调用仍然可以正常使用，但新功能可能无法使用。
        internal static readonly Version apiVersion = new Version(1, 4, 2);

        internal static string versionString;

        private static Mod clickerClass;

        internal static Mod ClickerClass
        {
            get
            {
                if (clickerClass == null && ModLoader.TryGetMod("ClickerClass", out var mod))
                {
                    clickerClass = mod;
                }
                return clickerClass;
            }
        }

        public override void Load()
        {
            versionString = apiVersion.ToString();
        }

        public override void Unload()
        {
            clickerClass = null;
            versionString = null;
        }

        //以下是可用的调用列表。在需要/推荐的位置调用它们，例如：ClickerCompat.SetClickerWeaponDefaults(item);
        //如果有返回值，当 Clicker Class 未加载时会尝试返回一个合理的默认值。
        //如果出现异常情况，请检查日志！
        #region 通用调用
        /// <summary>
        /// 在 <see cref="ModItem.SetDefaults"/> 中调用，为点击器武器设置重要的默认字段。设置的字段包括：
        /// DamageType、useTime、useAnimation、useStyle、holdStyle、noMelee、shoot、shootSpeed。
        /// 除非你知道自己在做什么，否则不要在之后修改它们！
        /// </summary>
        /// <param name="item">需要设置默认值的 <see cref="Item"/></param>
        internal static void SetClickerWeaponDefaults(Item item)
        {
            ClickerClass?.Call("SetClickerWeaponDefaults", versionString, item);
        }

        /// <summary>
        /// 在 <see cref="ModItem.SetDefaults"/> 中调用，为"音效按钮"设置重要的默认字段。设置的字段包括：
        /// maxStack。
        /// 除非你知道自己在做什么，否则不要在之后修改它们！
        /// </summary>
        /// <param name="item">需要设置默认值的 <see cref="Item"/></param>
        internal static void SetSFXButtonDefaults(Item item)
        {
            ClickerClass?.Call("SetSFXButtonDefaults", versionString, item);
        }

        /// <summary>
        /// 在 <see cref="ModProjectile.SetDefaults"/> 中调用，为点击器弹幕设置重要的默认字段。设置的字段包括：
        /// DamageType。
        /// 除非你知道自己在做什么，否则不要在之后修改它们！
        /// </summary>
        /// <param name="proj">需要设置默认值的 <see cref="Projectile"/></param>
        internal static void SetClickerProjectileDefaults(Projectile proj)
        {
            ClickerClass?.Call("SetClickerProjectileDefaults", versionString, proj);
        }

        /// <summary>
        /// 在 <see cref="ModType.SetStaticDefaults"/> 中调用，将此弹幕注册到"点击器职业"类别中
        /// </summary>
        /// <param name="modProj">需要注册的 <see cref="ModProjectile"/></param>
        internal static void RegisterClickerProjectile(ModProjectile modProj)
        {
            ClickerClass?.Call("RegisterClickerProjectile", versionString, modProj);
        }

        /// <summary>
        /// 在 <see cref="ModType.SetStaticDefaults"/> 中调用，将此弹幕注册到"点击器武器"类别中。
        /// <br>仅用于点击器直接生成的弹幕（Item.shoot）。Clicker Class 的所有点击器只使用一种此类弹幕。除非你知道自己在做什么，否则不要使用！</br>
        /// <br>各种效果只会在"点击时"通过检查此类别来触发，而不是检查"所有点击器职业弹幕"</br>
        /// </summary>
        /// <param name="modProj">需要注册的 <see cref="ModProjectile"/></param>
        internal static void RegisterClickerWeaponProjectile(ModProjectile modProj)
        {
            ClickerClass?.Call("RegisterClickerWeaponProjectile", versionString, modProj);
        }

        /// <summary>
        /// 在 <see cref="ModType.SetStaticDefaults"/> 中调用，将此物品注册到"点击器职业"类别中
        /// </summary>
        /// <param name="modItem">需要注册的 <see cref="ModItem"/></param>
        internal static void RegisterClickerItem(ModItem modItem)
        {
            ClickerClass?.Call("RegisterClickerItem", versionString, modItem);
        }

        /// <summary>
        /// 在 <see cref="ModType.SetStaticDefaults"/> 中调用，将此武器作为"点击器"注册到"点击器职业"类别中。<br/>
        /// 不要同时调用 <see cref="RegisterClickerItem"/>，因为此方法已经自动包含了该调用
        /// </summary>
        /// <param name="modItem">需要注册的 <see cref="ModItem"/></param>
        /// <param name="borderTexture">边框纹理的路径（可选）</param>
        internal static void RegisterClickerWeapon(ModItem modItem, string borderTexture = null)
        {
            ClickerClass?.Call("RegisterClickerWeapon", versionString, modItem, borderTexture);
        }

        /// <summary>
        /// 在 <see cref="ModType.SetStaticDefaults"/> 中调用，将此物品注册到"音效按钮"类别中。<br/>
        /// 当物品在背包中时会自动为激活的"音效按钮"做出贡献<br/>
        /// 不要同时调用 <see cref="RegisterClickerItem"/>，因为此方法已经自动包含了该调用
        /// </summary>
        /// <param name="modItem">需要注册的 <see cref="ModItem"/></param>
        /// <param name="playSoundAction">播放音效时执行的方法</param>
        internal static void RegisterSFXButton(ModItem modItem, Action<int> playSoundAction)
        {
            ClickerClass?.Call("RegisterSFXButton", versionString, modItem, playSoundAction);
        }

        /// <summary>
        /// 在 <see cref="Mod.PostSetupContent"/> 或 <see cref="ModType.SetStaticDefaults"/> 中调用，注册此点击效果
        /// </summary>
        /// <param name="mod">此效果所属的模组。只能使用你自己的模组实例！</param>
        /// <param name="internalName">效果的内部名称。会与关联的模组组合成唯一名称</param>
        /// <param name="amount">触发效果所需的点击数</param>
        /// <param name="colorFunc">在提示文本中代表此效果的（动态）文字颜色</param>
        /// <param name="action">效果触发时执行的方法</param>
        /// <param name="preHardMode">此效果是否主要属于困难模式前可获取的内容</param>
        /// <param name="nameArgs">需要绑定到显示名称的参数</param>
        /// <param name="descriptionArgs">需要绑定到描述的���数</param>
        /// <returns>唯一标识符，如果发生异常则返回 null。请检查日志！</returns>
        internal static string RegisterClickEffect(Mod mod, string internalName, int amount, Func<Color> colorFunc, Action<Player, EntitySource_ItemUse_WithAmmo, Vector2, int, int, float> action, bool preHardMode = false, object[] nameArgs = null, object[] descriptionArgs = null)
        {
            return ClickerClass?.Call("RegisterClickEffect", versionString, mod, internalName, amount, colorFunc, action, preHardMode, nameArgs, descriptionArgs) as string;
        }

        /// <summary>
        /// 在 <see cref="Mod.PostSetupContent"/> 或 <see cref="ModType.SetStaticDefaults"/> 中调用，注册此点击效果
        /// </summary>
        /// <param name="mod">此效果所属的模组。只能使用你自己的模组实例！</param>
        /// <param name="internalName">效果的内部名称。会与关联的模组组合成唯一名称</param>
        /// <param name="amount">触发效果所需的点击数</param>
        /// <param name="color">在提示文本中代表此效果的文字颜色</param>
        /// <param name="action">效果触发时执行的方法</param>
        /// <param name="preHardMode">此效果是否主要属于困难模式前可获取的内容</param>
        /// <param name="nameArgs">需要绑定到显示名称的参数</param>
        /// <param name="descriptionArgs">需要绑定到描述的���数</param>
        /// <remarks>如需动态颜色，请使用 Func[Color] 重载</remarks>
        /// <returns>唯一标识符，如果发生异常则返回 null。请检查日志！</returns>
        internal static string RegisterClickEffect(Mod mod, string internalName, int amount, Color color, Action<Player, EntitySource_ItemUse_WithAmmo, Vector2, int, int, float> action, bool preHardMode = false, object[] nameArgs = null, object[] descriptionArgs = null)
        {
            return RegisterClickEffect(mod, internalName, amount, () => color, action, preHardMode, nameArgs, descriptionArgs);
        }

        /// <summary>
        /// 返回此类型物品的边框纹理
        /// </summary>
        /// <param name="type">物品类型</param>
        /// <returns>边框纹理的路径，如果未找到则返回 null</returns>
        internal static string GetPathToBorderTexture(int type)
        {
            return ClickerClass?.Call("GetPathToBorderTexture", versionString, type) as string;
        }

        /// <summary>
        /// 返回所有现有效果的内部名称
        /// </summary>
        /// <returns>IEnumerable[string]</returns>
        internal static List<string> GetAllEffectNames()
        {
            return ClickerClass?.Call("GetAllEffectNames", versionString) as List<string>;
        }

        /// <summary>
        /// 访问某个效果的统计信息。如果未找到则返回 <see cref="null"/>。
        /// "Mod"：效果所属的模组 (Mod)。
        /// | "InternalName"：内部名称 (string)。
        /// | "UniqueName"：唯一名称 (string)（应与输入的字符串匹配）。
        /// | "DisplayName"：显示名称 (LocalizedText)。
        /// | "Description"：描述 (LocalizedText)。
        /// | "Amount"：触发效果所需的点击数 (int)。
        /// | "ColorFunc"：调用时返回的颜色 (Color)。
        /// | "Action"：触发时执行的方法 (Action[Player, EntitySource_ItemUse_WithAmmo, Vector2, int, int, float])。
        /// | "PreHardMode"：是否属于困难模式前可获取的内容 (bool)。
        /// </summary>
        /// <param name="effect">唯一效果名称</param>
        /// <returns>Dictionary[string, object]</returns>
        internal static Dictionary<string, object> GetClickEffectAsDict(string effect)
        {
            return ClickerClass?.Call("GetClickEffectAsDict", versionString, effect) as Dictionary<string, object>;
        }

        /// <summary>
        /// 检查此名称的效果是否存在
        /// </summary>
        /// <param name="effect">唯一名称</param>
        /// <returns>如果有效则返回 <see langword="true"/></returns>
        internal static bool IsClickEffect(string effect)
        {
            return ClickerClass?.Call("IsClickEffect", versionString, effect) as bool? ?? false;
        }

        /// <summary>
        /// 调用此方法检查某个弹幕类型是否属于"点击器职业"类别
        /// </summary>
        /// <param name="type">需要检查的物品类型</param>
        /// <returns>如果属于该类别则返回 <see langword="true"/></returns>
        internal static bool IsClickerProj(int type)
        {
            return ClickerClass?.Call("IsClickerProj", versionString, type) as bool? ?? false;
        }

        /// <summary>
        /// 调用此方法检查某个弹幕是否属于"点击器职业"类别
        /// </summary>
        /// <param name="proj">需要检查的 <see cref="Projectile"/></param>
        /// <returns>如果属于该类别则返回 <see langword="true"/></returns>
        internal static bool IsClickerProj(Projectile proj)
        {
            return ClickerClass?.Call("IsClickerProj", versionString, proj) as bool? ?? false;
        }

        /// <summary>
        /// 调用此方法检查某个弹幕类型是否属于"点击器武器"类别。
        /// <br>各种效果只会在"点击时"通过检查此类别来触发，而不是检查"所有点击器职业弹幕"</br>
        /// </summary>
        /// <param name="type">需要检查的物品类型</param>
        /// <returns>如果属于该类别则返回 <see langword="true"/></returns>
        internal static bool IsClickerWeaponProj(int type)
        {
            return ClickerClass?.Call("IsClickerWeaponProj", versionString, type) as bool? ?? false;
        }

        /// <summary>
        /// 调用此方法检查某个弹幕是否属于"点击器武器"类别。
        /// <br>各种效果只会在"点击时"通过检查此类别来触发，而不是检查"所有点击器职业弹幕"</br>
        /// </summary>
        /// <param name="proj">需要检查的 <see cref="Projectile"/></param>
        /// <returns>如果属于该类别则返回 <see langword="true"/></returns>
        internal static bool IsClickerWeaponProj(Projectile proj)
        {
            return ClickerClass?.Call("IsClickerWeaponProj", versionString, proj) as bool? ?? false;
        }

        /// <summary>
        /// 调用此方法检查某个物品类型是否属于"点击器职业"类别
        /// </summary>
        /// <param name="type">需要检查的物品类型</param>
        /// <returns>如果属于该类别则返回 <see langword="true"/></returns>
        internal static bool IsClickerItem(int type)
        {
            return ClickerClass?.Call("IsClickerItem", versionString, type) as bool? ?? false;
        }

        /// <summary>
        /// 调用此方法检查某个物品是否属于"点击器职业"类别
        /// </summary>
        /// <param name="item">需要检查的 <see cref="Item"/></param>
        /// <returns>如果是"点击器职业"物品则返回 <see langword="true"/></returns>
        internal static bool IsClickerItem(Item item)
        {
            return ClickerClass?.Call("IsClickerItem", versionString, item) as bool? ?? false;
        }

        /// <summary>
        /// 调用此方法检查某个物品是否为"音效按钮"
        /// </summary>
        /// <param name="item">需要检查的物品</param>
        /// <returns>如果是"音效按钮"则返回 <see langword="true"/></returns>
        internal static bool IsSFXButton(Item item)
        {
            return ClickerClass?.Call("IsSFXButton", versionString, item) as bool? ?? false;
        }

        /// <summary>
        /// 调用此方法检查某个物品类型是否为"音效按钮"
        /// </summary>
        /// <param name="type">需要检查的物品类型</param>
        /// <returns>如果是"音效按钮"则返回 <see langword="true"/></returns>
        internal static bool IsSFXButton(int type)
        {
            return ClickerClass?.Call("IsSFXButton", versionString, type) as bool? ?? false;
        }

        /// <summary>
        /// 调用此方法获取"音效按钮"的音效播放方法
        /// </summary>
        /// <param name="item">需要检查的物品</param>
        /// <returns>如果不是"音效按钮"则返回 <see langword="null"/></returns>
        internal static Action<int> GetSFXButton(Item item)
        {
            return ClickerClass?.Call("GetSFXButton", versionString, item) as Action<int> ?? null;
        }

        /// <summary>
        /// 调用此方法获取"音效按钮"的音效播放方法
        /// </summary>
        /// <param name="type">需要检查的物品类型</param>
        /// <returns>如果不是"音效按钮"则返回 <see langword="null"/></returns>
        internal static Action<int> GetSFXButton(int type)
        {
            return ClickerClass?.Call("GetSFXButton", versionString, type) as Action<int> ?? null;
        }

        /// <summary>
        /// 调用此方法检查某个物品类型是否为"点击器"
        /// </summary>
        /// <param name="type">需要检查的物品类型</param>
        /// <returns>如果是"点击器"则返回 <see langword="true"/></returns>
        internal static bool IsClickerWeapon(int type)
        {
            return ClickerClass?.Call("IsClickerWeapon", versionString, type) as bool? ?? false;
        }

        /// <summary>
        /// 调用此方法检查某个物品是否为"点击器"
        /// </summary>
        /// <param name="item">需要检查的 <see cref="Item"/></param>
        /// <returns>如果是"点击器"则返回 <see langword="true"/></returns>
        internal static bool IsClickerWeapon(Item item)
        {
            return ClickerClass?.Call("IsClickerWeapon", versionString, item) as bool? ?? false;
        }
        #endregion

        #region 物品调用
        /// <summary>
        /// 在 <see cref="ModItem.SetDefaults"/> 中为点击器武器调用，设置其半径颜色
        /// </summary>
        /// <param name="item">点击器武器</param>
        /// <param name="color">颜色</param>
        internal static void SetColor(Item item, Color color)
        {
            ClickerClass?.Call("SetColor", versionString, item, color);
        }

        /// <summary>
        /// 在 <see cref="ModItem.SetDefaults"/> 中为点击器武器调用，设置其特定半径增量（1f 表示 100 像素）
        /// </summary>
        /// <param name="item">点击器武器</param>
        /// <param name="radius">额外半径</param>
        internal static void SetRadius(Item item, float radius)
        {
            ClickerClass?.Call("SetRadius", versionString, item, radius);
        }

        /// <summary>
        /// 在 <see cref="ModItem.SetDefaults"/> 中为点击器武器调用，为其添加一个效果
        /// </summary>
        /// <param name="item">点击器武器</param>
        /// <param name="effect">唯一效果名称</param>
        internal static void AddEffect(Item item, string effect)
        {
            ClickerClass?.Call("AddEffect", versionString, item, effect);
        }

        /// <summary>
        /// 在 <see cref="ModItem.SetDefaults"/> 中为点击器武器调用，为其添加多个效果
        /// </summary>
        /// <param name="item">点击器武器</param>
        /// <param name="effects">效果名称集合</param>
        internal static void AddEffect(Item item, IEnumerable<string> effects)
        {
            ClickerClass?.Call("AddEffect", versionString, item, effects);
        }

        /// <summary>
        /// 在 <see cref="ModItem.SetDefaults"/> 中为点击器武器调用，设置使用时产生的粒子类型
        /// </summary>
        /// <param name="item">点击器武器</param>
        /// <param name="type">粒子类型</param>
        internal static void SetDust(Item item, int type)
        {
            ClickerClass?.Call("SetDust", versionString, item, type);
        }

        /// <summary>
        /// 在 <see cref="ModItem.SetDefaults"/> 中为点击器物品调用，为其分配一个饰品类型，使得具有相同饰品类型的物品无法同时装备。支持的类型：
        /// ClickingGlove
        /// </summary>
        /// <param name="item">点击器职业物品</param>
        internal static void SetAccessoryType(Item item, string accessoryType)
        {
            ClickerClass?.Call("SetAccessoryType", versionString, item, accessoryType);
        }

        /// <summary>
        /// 在 <see cref="ModItem.SetDefaults"/> 中为点击器物品调用，使其在提示文本中显示总点击数
        /// </summary>
        /// <param name="item">点击器职业物品</param>
        internal static void SetDisplayTotalClicks(Item item)
        {
            ClickerClass?.Call("SetDisplayTotalClicks", versionString, item);
        }

        /// <summary>
        /// 在 <see cref="ModItem.SetDefaults"/> 中为点击器物品调用，使其在提示文本中显示总金币生成量
        /// </summary>
        /// <param name="item">点击器职业物品</param>
        internal static void SetDisplayMoneyGenerated(Item item)
        {
            ClickerClass?.Call("SetDisplayMoneyGenerated", versionString, item);
        }
        #endregion

        #region 玩家调用
        [Obsolete("请改用 GetClickerRadiusNew", error: false)]
        /// <summary>
        /// 调用此方法获取玩家的点击器半径（乘以 100 得到像素值）
        /// </summary>
        /// <param name="player">玩家</param>
        internal static float GetClickerRadius(Player player)
        {
            return ClickerClass?.Call("GetPlayerStat", versionString, player, "clickerRadius") as float? ?? 1f;
        }

        /// <summary>
        /// 调用此方法获取玩家的点击器半径修正值（调用 ApplyTo(100) 可获取像素半径）
        /// </summary>
        /// <param name="player">玩家</param>
        internal static StatModifier GetClickerRadiusNew(Player player)
        {
            return ClickerClass?.Call("GetPlayerStat", versionString, player, "ClickerRadius") as StatModifier? ?? StatModifier.Default;
        }

        /// <summary>
        /// 调用此方法获取玩家的点击数（已经完成了多少次点击）
        /// </summary>
        /// <param name="player">玩家</param>
        internal static int GetClickAmount(Player player)
        {
            return ClickerClass?.Call("GetPlayerStat", versionString, player, "clickAmount") as int? ?? 0;
        }

        /// <summary>
        /// 调用此方法获取玩家每秒点击数。如需整数请使用 Math.Floor。
        /// </summary>
        /// <param name="player">玩家</param>
        internal static float GetClickerPerSecond(Player player)
        {
            return ClickerClass?.Call("GetPlayerStat", versionString, player, "clickerPerSecond") as float? ?? 0;
        }

        /// <summary>
        /// 调用此方法获取玩家在物品上触发指定效果所需的总点击数。
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="item">点击器物品</param>
        /// <param name="effect">唯一效果名称</param>
        internal static int GetClickerAmountTotal(Player player, Item item, string effect)
        {
            return ClickerClass?.Call("GetPlayerStat", versionString, player, "clickerAmountTotal", item, effect) as int? ?? 1;
        }

        /// <summary>
        /// 调用此方法检查玩家是否穿戴了特定套装。支持的套装：
        /// Motherboard、Overclock、Precursor、Mice、RGB
        /// </summary>
        /// <param name="player">玩家</param>
        internal static bool GetArmorSet(Player player, string set)
        {
            return ClickerClass?.Call("GetArmorSet", versionString, player, set) as bool? ?? false;
        }

        /// <summary>
        /// 调用此方法检查特定饰品效果是否启用（例如"玩家礼盒"会启用多个效果）。支持的饰品：
        /// ChocolateChip、EnchantedLED、EnchantedLED2、StickyKeychain、GlassOfMilk、CookieVisual、CookieVisual2、ClickingGlove、AncientClickingGlove、RegalClickingGlove、PortableParticleAccelerator、MouseTrap、HotKeychain、TriggerFinger、ButtonMasher、AimAssistModule、AimbotModule。
        /// </summary>
        /// <param name="player">玩家</param>
        internal static bool GetAccessory(Player player, string accessory)
        {
            return ClickerClass?.Call("GetAccessory", versionString, player, accessory) as bool? ?? false;
        }

        /// <summary>
        /// 调用此方法设置特定玩家饰品效果（例如要模拟"玩家礼盒"需要设置多个效果）。支持的饰品：
        /// ChocolateChip、EnchantedLED、EnchantedLED2、StickyKeychain、GlassOfMilk、CookieVisual、CookieVisual2、ClickingGlove、AncientClickingGlove、RegalClickingGlove、PortableParticleAccelerator、MouseTrap、HotKeychain、TriggerFinger、ButtonMasher、AimAssistModule、AimbotModule。
        /// </summary>
        /// <param name="player">玩家</param>
        internal static void SetAccessory(Player player, string accessory)
        {
            ClickerClass?.Call("SetAccessory", versionString, player, accessory);
        }

        /// <summary>
        /// 调用此方法检查生成弹幕的特定饰品效果是否启用。如果启用则返回对应物品。支持的饰品：
        /// Cookie、AMedal、SMedal、FMedal、GoldenTicket、BottomlessBoxOfPaperclips。
        /// </summary>
        /// <param name="player">玩家</param>
        internal static Item GetAccessoryItem(Player player, string accessory)
        {
            return ClickerClass?.Call("GetAccessoryItem", versionString, player, accessory) as Item ?? null;
        }

        /// <summary>
        /// 调用此方法设置生成弹幕的特定玩家饰品效果。支持的饰品：
        /// Cookie、AMedal、SMedal、FMedal、GoldenTicket、BottomlessBoxOfPaperclips。
        /// </summary>
        /// <param name="player">玩家</param>
        internal static void SetAccessoryItem(Player player, string accessory, Item item)
        {
            ClickerClass?.Call("SetAccessoryItem", versionString, player, accessory, item);
        }

        /// <summary>
        /// 调用此方法增加玩家的点击器暴击值
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="add">增加的暴击率</param>
        internal static void SetClickerCritAdd(Player player, int add)
        {
            ClickerClass?.Call("SetPlayerStat", versionString, player, "clickerCritAdd", add);
        }

        /// <summary>
        /// 调用此方法增加玩家的点击器固定伤害值
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="add">增加的固定伤害</param>
        internal static void SetDamageFlatAdd(Player player, int add)
        {
            ClickerClass?.Call("SetPlayerStat", versionString, player, "clickerDamageFlatAdd", add);
        }

        /// <summary>
        /// 调用此方法按百分比增加玩家的点击器伤害值
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="add">增加的伤害百分比</param>
        internal static void SetDamageAdd(Player player, float add)
        {
            ClickerClass?.Call("SetPlayerStat", versionString, player, "clickerDamageAdd", add);
        }

        /// <summary>
        /// 调用此方法修改玩家的效果触发阈值
        /// （2 表示需要减少 2 次点击才能达到效果触发阈值）
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="add">减少的点击数</param>
        internal static void SetClickerBonusAdd(Player player, int add)
        {
            ClickerClass?.Call("SetPlayerStat", versionString, player, "clickerBonusAdd", add);
        }

        /// <summary>
        /// 调用此方法修改玩家的效果触发阈值
        /// （-0.20f 表示需要减少 20% 的点击才能达到效果触发阈值）
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="add">总点击数的百分比</param>
        internal static void SetClickerBonusPercentAdd(Player player, float add)
        {
            ClickerClass?.Call("SetPlayerStat", versionString, player, "clickerBonusPercentAdd", add);
        }

        /// <summary>
        /// 调用此方法增加玩家的点击器半径（默认 1f），下限为点击器武器的基础半径
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="add">增加的距离，以 100 像素为单位（1f = 100 像素）</param>
        internal static void SetClickerRadiusAdd(Player player, float add)
        {
            ClickerClass?.Call("SetPlayerStat", versionString, player, "clickerRadiusAdd", add);
        }

        /// <summary>
        /// 调用此方法乘以玩家的点击器半径，同时影响加成和点击器武器的基础半径。<br/>
        /// 通常用于使用小于 1 的值进行缩小
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="mult">半径的倍率</param>
        internal static void SetClickerRadiusMult(Player player, float mult)
        {
            ClickerClass?.Call("SetPlayerStat", versionString, player, "clickerRadiusMult", mult);
        }

        /// <summary>
        /// 为此玩家启用某个点击效果
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="effect">唯一效果名称</param>
        internal static void EnableClickEffect(Player player, string effect)
        {
            ClickerClass?.Call("EnableClickEffect", versionString, player, effect);
        }

        /// <summary>
        /// 为此玩家启用多个点击效果
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="effects">唯一效果名称集合</param>
        internal static void EnableClickEffect(Player player, IEnumerable<string> effects)
        {
            ClickerClass?.Call("EnableClickEffect", versionString, player, effects);
        }

        /// <summary>
        /// 检查玩家是否启用了某个点击效果
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="effect">唯一效果名称</param>
        /// <returns>如果已启用则返回 <see langword="true"/></returns>
        internal static bool HasClickEffect(Player player, string effect)
        {
            return ClickerClass?.Call("HasClickEffect", versionString, player, effect) as bool? ?? false;
        }

        /// <summary>
        /// 返回当前所有激活的"音效按钮"堆叠数。<br/>
        /// 配合 <see cref="GetSFXButton"/> 使用来获取音效
        /// </summary>
        /// <param name="player">玩家</param>
        /// <returns>映射物品类型到堆叠数的字典</returns>
        internal static IReadOnlyDictionary<int, int> GetAllSFXButtonStacks(Player player)
        {
            return ClickerClass?.Call("GetAllSFXButtonStacks", versionString, player) as IReadOnlyDictionary<int, int> ?? null;
        }

        /// <summary>
        /// 将指定 <paramref name="item"/> 的堆叠数加 1<br/>
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="item">物品</param>
        /// <returns>如果达到 5 则返回 <see langword="true"/></returns>
        internal static bool AddSFXButtonStack(Player player, Item item)
        {
            return ClickerClass?.Call("AddSFXButtonStack", versionString, player, item) as bool? ?? false;
        }

        /// <summary>
        /// 将指定物品类型的堆叠数增加 <paramref name="stack"/><br/>
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="type">物品类型</param>
        /// <param name="stack">要增加的堆叠数</param>
        /// <returns>如果达到 5 则返回 <see langword="true"/></returns>
        internal static bool AddSFXButtonStack(Player player, int type, int stack)
        {
            return ClickerClass?.Call("AddSFXButtonStack", versionString, player, type, stack) as bool? ?? false;
        }

        /// <summary>
        /// 为玩家设置一个自动连点效果。如果存在多个激活的效果，会对玩家应用最快的那一个
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="speedFactor">此效果激活时 useTime 的倍率，结果为 1。结果向下取整，最小为 2（游戏限制）。示例：6f -> 有效使用时间 = 1 * 6f -> 60 / 6f -> 10 次/秒</param>
        /// <param name="controlledByKeyBind">此效果只有在玩家使用 Clicker Class 专用快捷键切换时才会激活</param>
        /// <param name="preventsClickEffects">此效果激活时不会触发点击效果</param>
        internal static void SetAutoReuseEffect(Player player, float speedFactor, bool controlledByKeyBind = false, bool preventsClickEffects = false)
        {
            ClickerClass?.Call("SetAutoReuseEffect", versionString, player, speedFactor, controlledByKeyBind, preventsClickEffects);
        }
        #endregion
    }
}
