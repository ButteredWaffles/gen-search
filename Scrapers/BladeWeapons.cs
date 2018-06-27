using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Gensearch.Helpers;
using SQLite;

namespace Gensearch.Scrapers
{
    public class BladeWeapons
    {
        SQLiteAsyncConnection db = GenSearch.db;
        Regex intsOnly = new Regex(@"[^\d\+-]");

        /// <summary>
        /// Retrieves weapon information for greatswords, longswords, sword and shields, hammers, lances, and insect glaives.
        /// </summary>
        /// <param name="address">The URL of the weapon.</param>
        /// <para><see cref="GetHuntingHorn(string, int[])"></see> if you wish to gather information on hunting horns.</para>
        /// <para><see cref="GetPhialAndShellWeapons(string)"></see> if you wish to gather information on switch axes, charge blades, and gunlances..</para>
        public async Task GetGenericSword(string address) {
            try {
                var config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
                var context = BrowsingContext.New(config);
                var page = await context.OpenAsync(address);
                string setname = page.QuerySelector("[itemprop=\"name\"]").TextContent.Split("/")[0].Trim();
                ConsoleWriters.StartingPageMessage($"Started work on the {setname} series. ({address})");

                var crafting_table = page.QuerySelectorAll(".table")[1].QuerySelector("tbody");
                int current_wpn_index = 0;
                foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr")) {

                    SwordValues sv = await GetSwordAttributes(page, tr, crafting_table, current_wpn_index);
                    List<SharpnessValue> sharpvalues = GetSharpness(page, tr);
                    await db.InsertAllAsync(sharpvalues);
                    sv.sharp_0_id = sharpvalues[0].sharp_id;
                    sv.sharp_1_id = sharpvalues[1].sharp_id;
                    sv.sharp_2_id = sharpvalues[2].sharp_id;
                    sv.sword_set_name = setname;

                    if (address.Contains("/greatsword/")) { sv.sword_class = "Great Sword"; }
                    else if (address.Contains("/longsword/")) { sv.sword_class = "Long Sword"; }
                    else if (address.Contains("/swordshield/")) { sv.sword_class = "Sword & Shield"; }
                    else if (address.Contains("/hammer/")) { sv.sword_class = "Hammer"; }
                    else if (address.Contains("/lance/")) { sv.sword_class = "Great Sword"; }
                    else if (address.Contains("/insectglaive/")) {sv.sword_class = "Insect Glaive"; }
                    else if (address.Contains("/dualblades/")) {sv.sword_class = "Dual Blades";}
                    await db.InsertAsync(sv);

                    List<CraftItem> craftitems = Weapons.GetCraftItems(crafting_table.Children[current_wpn_index]);
                    foreach (CraftItem item in craftitems) {
                        item.creation_id = sv.sword_id;
                        item.creation_type = "Blademaster";
                    }
                    foreach (ElementDamage element in sv.element) {
                        element.weapon_id = sv.sword_id;
                    }
                    await db.InsertAllAsync(sv.element);
                    await db.InsertAllAsync(craftitems);
                    current_wpn_index++;
                }
                ConsoleWriters.CompletionMessage($"Finished with the {setname} series!");
            }
            catch (Exception ex) {
                ConsoleWriters.ErrorMessage(ex.ToString());
                await GetGenericSword(address);
            }
        }

        /// <summary>
        /// Retrieves weapon information for hunting horns.
        /// </summary>
        /// <param name="address">The URL of the weapon.</param>
        /// <param name="notes">An array of ints corresponding to the horn's note values.</param>
        /// <returns></returns>
        public async Task GetHuntingHorn(string address, int[] notes) {
            try {
                var config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
                var context = BrowsingContext.New(config);
                var page = await context.OpenAsync(address);
                string setname = page.QuerySelector("[itemprop=\"name\"]").TextContent.Split("/")[0].Trim();
                ConsoleWriters.StartingPageMessage($"Started work on the {setname} series. ({address})");
                string notestring = "";

                foreach (int note in notes) {
                    switch(note) {
                        case 1:
                            notestring += "white ";
                            break;
                        case 2:
                            notestring += "purple ";
                            break;
                        case 3:
                            notestring += "red ";
                            break;
                        case 4:
                            notestring += "blue ";
                            break;
                        case 5:
                            notestring += "green ";
                            break;
                        case 6:
                            notestring += "yellow ";
                            break;
                        case 7:
                            notestring += "light_blue ";
                            break;
                    }
                }

                notestring = notestring.Trim().Replace(" ", ", ");

                var crafting_table = page.QuerySelectorAll(".table")[1].QuerySelector("tbody");
                int current_wpn_index = 0;
                foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr")) {
                    SwordValues sv = await GetSwordAttributes(page, tr, crafting_table, current_wpn_index);
                    List<SharpnessValue> sharpvalues = GetSharpness(page, tr);
                    await db.InsertAllAsync(sharpvalues);
                    sv.sharp_0_id = sharpvalues[0].sharp_id;
                    sv.sharp_1_id = sharpvalues[1].sharp_id;
                    sv.sharp_2_id = sharpvalues[2].sharp_id;
                    sv.sword_set_name = setname;
                    sv.sword_class = "Hunting Horn";
                    await db.InsertAsync(sv);

                    List<CraftItem> craftitems = Weapons.GetCraftItems(crafting_table.Children[current_wpn_index]);
                    foreach (CraftItem item in craftitems) {
                        item.creation_id = sv.sword_id;
                        item.creation_type = "Blademaster";
                    }
                    foreach (ElementDamage element in sv.element) {
                        element.weapon_id = sv.sword_id;
                    }
                    await db.InsertAllAsync(sv.element);
                    await db.InsertAllAsync(craftitems);

                    HuntingHorn horn = new HuntingHorn() {
                        sword_id = sv.sword_id,
                        notes = notestring
                    };

                    await db.InsertAsync(horn);

                    current_wpn_index++;
                }
                ConsoleWriters.CompletionMessage($"Finished with the {setname} series!");
            }
            catch (Exception ex) {
                ConsoleWriters.ErrorMessage(ex.ToString());
                await GetHuntingHorn(address, notes);
            }
        }

        public async Task GetPhialAndShellWeapons(string address) {
            try {
                var config = Configuration.Default.WithDefaultLoader(l => l.IsResourceLoadingEnabled = true).WithCss();
                var context = BrowsingContext.New(config);
                var page = await context.OpenAsync(address);
                string setname = page.QuerySelector("[itemprop=\"name\"]").TextContent.Split("/")[0].Trim();
                ConsoleWriters.StartingPageMessage($"Started work on the {setname} series. ({address})");

                var crafting_table = page.QuerySelectorAll(".table")[1].QuerySelector("tbody");
                int current_wpn_index = 0;
                foreach (var tr in page.QuerySelector(".table").QuerySelectorAll("tr")) {

                    SwordValues sv = await GetSwordAttributes(page, tr, crafting_table, current_wpn_index);
                    List<SharpnessValue> sharpvalues = GetSharpness(page, tr);
                    await db.InsertAllAsync(sharpvalues);
                    sv.sharp_0_id = sharpvalues[0].sharp_id;
                    sv.sharp_1_id = sharpvalues[1].sharp_id;
                    sv.sharp_2_id = sharpvalues[2].sharp_id;
                    sv.sword_set_name = setname;

                    if (address.Contains("/chargeblade/")) { sv.sword_class = "Charge Blade"; }
                    else if (address.Contains("/gunlance/")) { sv.sword_class = "Gunlance"; }
                    else { sv.sword_class = "Switch Axe"; }
                    await db.InsertAsync(sv);

                    List<CraftItem> craftitems = Weapons.GetCraftItems(crafting_table.Children[current_wpn_index]);
                    foreach (CraftItem item in craftitems) {
                        item.creation_id = sv.sword_id;
                        item.creation_type = "Blademaster";
                    }
                    foreach (ElementDamage element in sv.element) {
                        element.weapon_id = sv.sword_id;
                    }
                    await db.InsertAllAsync(sv.element);
                    await db.InsertAllAsync(craftitems);

                    string phialtype = GetPhialType(tr, sv.sword_class);
                    PhialOrShellWeapon weapon = new PhialOrShellWeapon() {
                        sword_id = sv.sword_id,
                        phial_or_shell_type = phialtype
                    };
                    await db.InsertAsync(weapon);

                    current_wpn_index++;
                }
                ConsoleWriters.CompletionMessage($"Finished with the {setname} series!");
            }
            catch (Exception ex) {
                ConsoleWriters.ErrorMessage(ex.ToString());
                await GetPhialAndShellWeapons(address);
            }
        }
        
        /// <summary>
        /// Gets general weapon information. Blademaster only.
        /// </summary>
        /// <param name="page">The IDocument holding the page information.</param>
        /// <param name="wrapper">The table row element holding the weapon information.</param>
        /// <param name="crafting">The table containing information on price, upgrades, and items.</param>
        /// <param name="current_index">The index of the wrapper element in its parent table.</param>
        /// <returns>Returns a SwordValues object containing the retrieved information.</returns>
        public async Task<SwordValues> GetSwordAttributes(IDocument page, IElement wrapper, IElement crafting, int current_index) {
            string weapon_name = wrapper.FirstElementChild.TextContent;
            int weapon_damage = Convert.ToInt32(wrapper.Children[1].TextContent);
            var techinfo = wrapper.Children[5];
            int slots = techinfo.FirstElementChild.TextContent.Count(c => c == '◯');
            int rarity = Convert.ToInt32(techinfo.Children[1].TextContent.Trim().Replace("RARE", ""));
            string upgrades_into = "none";
            var upgradeinfo = crafting.Children[current_index].QuerySelectorAll("td");
            if (upgradeinfo[0].QuerySelector(".font-weight-bold") != null) {
                upgrades_into = String.Join('\n', upgradeinfo[0].QuerySelectorAll("a").Select(a => a.TextContent.Trim()));
            }
            List<ElementDamage> elements = new List<ElementDamage>();
            int affinity = 0;
            foreach (var small in wrapper.Children[2].QuerySelectorAll("small")) {
                if (small.TextContent.Any(char.IsLetter)) {
                    elements.Add(Weapons.GetElement(small));
                }
                else {
                    affinity = Convert.ToInt32(intsOnly.Replace(small.TextContent.Trim(), ""));
                }
            }
            int price = Convert.ToInt32(upgradeinfo[1].TextContent.Replace("z", ""));
            int monsterid = -1;
            if (page.QuerySelectorAll(".lead").Count() == 3) {
                monsterid = (await Monsters.GetMonsterFromDB(page.QuerySelectorAll(".lead")[2].TextContent.Trim())).id;
            }
            return new SwordValues() {
                sword_name = weapon_name,
                raw_dmg = weapon_damage,
                slots = slots,
                rarity = rarity,
                upgrades_into = upgrades_into,
                price = price,
                element = elements,
                affinity = affinity,
                monster_id = monsterid,
            };
        }

        /// <summary>
        /// Retrieves weapon sharpness information. Blademaster only.
        /// </summary>
        /// <param name="page">The IDocument holding the page information.</param>
        /// <param name="wrapper">The table row element holding the weapon information.</param>
        /// <returns>
        /// <para>Returns a list of SharpnessValue objects.</para>
        /// <para>Index zero is the base weapon sharpness, index one is the sharpness with the skill Sharpness+1, and index two
        /// is the sharpness with Sharpness+2.</para>
        /// </returns>
        public List<SharpnessValue> GetSharpness(IDocument page, IElement wrapper) {
            List<SharpnessValue> values = new List<SharpnessValue>();
            var sharpvalues = wrapper.Children[3].QuerySelectorAll("div");
            for (int i = 0; i <= 2; i++) {
                var spans = sharpvalues[i].QuerySelectorAll("span");
                int red_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[0]).Width, "")) * 5;
                int orange_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[1]).Width, "")) * 5;
                int yellow_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[2]).Width, "")) * 5;
                int green_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[3]).Width, "")) * 5;
                int blue_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[4]).Width, "")) * 5;
                int white_sharpness = Convert.ToInt32(intsOnly.Replace(page.DefaultView.GetComputedStyle(spans[5]).Width, "")) * 5;
                values.Add(new SharpnessValue() {
                    handicraft_modifier = i,
                    red_sharpness_length = red_sharpness,
                    orange_sharpness_length = orange_sharpness,
                    yellow_sharpness_length = yellow_sharpness,
                    green_sharpness_length = green_sharpness,
                    blue_sharpness_length = blue_sharpness,
                    white_sharpness_length = white_sharpness
                });
            }
            return values;
        }

        /// <summary>
        /// Get the phial and/or shell name from a weapon. For charge blades, switch axes, and gunlances.
        /// </summary>
        /// <param name="wrapper">The table row element containing the weapon information.</param>
        /// <param name="type">The type of weapon.</param>
        /// <returns>Returns a string with the phial type.</returns>
        public string GetPhialType(IElement wrapper, string type) {
            var phialinfo = wrapper.Children[4].TextContent;
            if (type == "Gunlance") {return phialinfo.Trim(); }
            else { return phialinfo.Split(':')[1].Trim(); }
        }
    }
}