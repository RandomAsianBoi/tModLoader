using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader.Default.Developer;
using Terraria.ModLoader.Default.Patreon;

namespace Terraria.ModLoader.Default
{
	internal class ModLoaderMod : Mod
	{
		private static bool texturesLoaded;
		private static Texture2D mysteryItemTexture;
		private static Texture2D startBagTexture;
		private static Texture2D mysteryTileTexture;

		public override string Name => "ModLoader";
		public override Version Version => ModLoader.version;
		public override Version tModLoaderVersion => ModLoader.version;

		internal ModLoaderMod()
		{
			Side = ModSide.NoSync;
			DisplayName = "tModLoader";
		}

		public override void Load()
		{
			LoadTextures();
			AddTexture("MysteryItem", mysteryItemTexture);
			AddTexture("StartBag", startBagTexture);
			AddTexture("MysteryTile", mysteryTileTexture);
			AddItem("MysteryItem", new MysteryItem());
			AddGlobalItem("MysteryGlobalItem", new MysteryGlobalItem());
			AddItem("StartBag", new StartBag());
			AddItem("AprilFools", new AprilFools());
			AddTile("MysteryTile", new MysteryTile(), "ModLoader/MysteryTile");
			AddTile("PendingMysteryTile", new MysteryTile(), "ModLoader/MysteryTile");
			AddTileEntity("MysteryTileEntity", new MysteryTileEntity());
			AddPlayer("MysteryPlayer", new MysteryPlayer());
			AddModWorld("MysteryWorld", new MysteryWorld());
			AddModWorld("MysteryTilesWorld", new MysteryTilesWorld());
			AddCommand("HelpCommand", new HelpCommand());
			AddCommand("ModlistCommand", new ModlistCommand());
			AddPatronSets();
			AddPlayer("PatronModPlayer", new PatronModPlayer());
			AddDeveloperSets();
			AddPlayer("DeveloperPlayer", new DeveloperPlayer());
		}

		// If new types arrise (probably not), change the format:
		// head, body, legs, wings, <new>
		private static readonly PatreonItem[][] PatronSets =
		{
			new PatreonItem[] { new toplayz_Head(), new toplayz_Body(), new toplayz_Legs() },
			new PatreonItem[] { new KittyKitCatCat_Head(), new KittyKitCatCat_Body(), new KittyKitCatCat_Legs() },
			new PatreonItem[] { new Polyblank_Head(), new Polyblank_Body(), new Polyblank_Legs() },
			new PatreonItem[] { new dinidini_Head(), new dinidini_Body(), new dinidini_Legs(), new dinidini_Wings() },
			new PatreonItem[] { new Remeus_Head(), new Remeus_Body(), new Remeus_Legs() },
			new PatreonItem[] { new Saethar_Head(), new Saethar_Body(), new Saethar_Legs(), new Saethar_Wings(),  },
			new PatreonItem[] { new Orian_Head(), new Orian_Body(), new Orian_Legs()  },
			new PatreonItem[] { new Squid_Head(), new Squid_Body(), new Squid_Legs()  },
			new PatreonItem[] { new Glory_Head(), new Glory_Body(), new Glory_Legs()  }
		};

		private void AddPatronSets()
		{
			// Flatten, and select items not null
			foreach (var patronItem in PatronSets.SelectMany(x => x))
			{
				AddItemAndEquipType(patronItem, "Patreon", patronItem.SetName, patronItem.ItemEquipType);
			}
		}

		private static readonly DeveloperItem[][] DeveloperSets =
		{
			new DeveloperItem[] { new PowerRanger_Head(), new PowerRanger_Body(), new PowerRanger_Legs() }
		};


		private void AddDeveloperSets()
		{
			// Flatten, and select items not null
			foreach (var developerItem in DeveloperSets.SelectMany(x => x))
			{
				AddItemAndEquipType(developerItem, "Developer", developerItem.SetName, developerItem.ItemEquipType);
			}
		}

		// Adds the given patreon item to ModLoader, and handles loading its assets automatically
		private void AddItemAndEquipType(ModItem item, string prefix, string name, EquipType equipType)
		{
			// If a client, we need to add several textures
			if (!Main.dedServ)
			{
				AddTexture($"{prefix}.{name}_{equipType}", ReadTexture($"{prefix}.{name}_{equipType}"));
				AddTexture($"{prefix}.{name}_{equipType}_{equipType}", ReadTexture($"{prefix}.{name}_{equipType}_{equipType}"));
				if (equipType == EquipType.Body) // If a body, add the arms texture
				{
					AddTexture($"{prefix}.{name}_{equipType}_Arms", ReadTexture($"{prefix}.{name}_{equipType}_Arms"));
				}
			}
			// Adds the item to ModLoader, as well as the normal assets
			AddItem($"{name}_{equipType}", item);
			// AddEquipTexture adds the arms and female body assets automatically, if EquipType is Body
			AddEquipTexture(item, equipType, item.Name, item.Texture + '_' + equipType, item.Texture + "_Arms", item.Texture + "_FemaleBody");
		}

		private static void LoadTextures()
		{
			if (Main.dedServ)
			{
				return;
			}
			mysteryItemTexture = ReadTexture("MysteryItem");
			startBagTexture = ReadTexture("StartBag");
			mysteryTileTexture = ReadTexture("MysteryTile");
			texturesLoaded = true;
		}

		internal static Texture2D ReadTexture(string file)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			// if someone set the type or name wrong, the stream will be null.
			Stream stream = assembly.GetManifestResourceStream("Terraria.ModLoader.Default." + file + ".png");
			if (stream == null) // [sanity check, makes it easier to know what's wrong]
			{
				throw new ArgumentException("Given EquipType for PatreonItem or name is not valid. It is possible either does not match up with the classname. If you added a new EquipType, modify GetEquipTypeSuffix() and AddPatreonItemAndEquipType() first.");
			}

			return Texture2D.FromStream(Main.instance.GraphicsDevice, stream);
		}


		private const int ChanceToGetArmor = 20;

		internal static bool TryGettingPatreonArmor(Player player)
		{
			if (Main.rand.NextBool(ChanceToGetArmor))
			{
				int randomIndex = Main.rand.Next(PatronSets.Length);

				foreach (var patreonItem in PatronSets[randomIndex])
				{
					player.QuickSpawnItem(patreonItem.item.type);
				}

				return true;
			}
			return false;
		}
	}
}
