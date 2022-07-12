(() => {

    function extract(keyword, source) {
        if (!source) return '';

        let index = source.toUpperCase().indexOf(keyword.toUpperCase());
        if (index === -1) index = source.indexOf(keyword.toUpperCase());
        if (index === -1) return '';

        let result = source.slice(index + keyword.length, source.length);
        result = result.replaceAll(' ', '');

        const firstNumber = result.match(/\d+/);
        if (!firstNumber) return '';
        const firstNumberIndex = result.indexOf(firstNumber[0]);
        if (firstNumberIndex > 3) return ''
        else if (firstNumberIndex > 0 && result[firstNumberIndex - 1] === '-') return '-' + firstNumber[0];
        else return firstNumber[0];

    }

    function newKeywords(sourceKeywords, char) {
        let prefixed = sourceKeywords.map(x => char + x);
        let postfixed = sourceKeywords.map(x => x + char);
        return [...prefixed, ...postfixed];
    }

    function getValue(keywords, source) {
        let postfixKeywords = [...newKeywords(keywords, ' '), ...newKeywords(keywords, ':'), ...newKeywords(keywords, '\n')];

        let results = postfixKeywords.map(keyword => extract(keyword, source));
        return results.find(value => !isNaN(value) && value != '');
    }

    function hasValue(keyword, source) {
        return source.toUpperCase().includes(keyword.toUpperCase());
    }

    function convert(val, is5e) {
        const strKeywords = ['str', 'strength'];
        const dexKeywords = ['dex', 'dexterity'];
        const intKeywords = ['int', 'intelligence'];
        const chaKeywords = ['cha', 'charisma'];
        const dmgKeywords = ['hit', 'damage', 'dmg'];
        const hitpoints = ['hitpoints', 'hit points', 'hp'];
        const armorKeywords = ['AC', 'Armor Class', "ArmorClass", "Armor"];
        const rangedConversionKeywords = ['Ranged Weapon'];

        const statKeywords = ['stat', 'stats', 'statistics', 'statistic'];
        const heartKeywords = ['heart', 'hearts'];

        const thKeywords = ['th','rth'];
        const ohKeywords = ['oh','roh'];
        const nwKeywords = ['nw'];
        const rangedKeywords = ['roh','rth'];

        const convertModifier = (mod) => {
            let fmod = Math.ceil((Number(mod) - 10) / 2);
            fmod = Math.ceil(fmod / 3);
            return fmod < 0 ? 0 : fmod;
        }

        const averageMod = (creature) => {
            let match = null;
            let arr = [creature.strength, creature.intelligence, creature.dexterity, creature.charisma];
            arr.sort().reduce((prev, curr) => curr === prev ? match = curr : curr);
            return match !== null ? match : arr[0];
        }

        const getVariance = (avg, creature) => {
            let variance = '';
            if (creature.strength != avg) variance += `STR:+${creature.strength} `;
            if (creature.charisma != avg) variance += `CHA:+${creature.charisma} `;
            if (creature.dexterity != avg) variance += `DEX:+${creature.dexterity} `;
            if (creature.intelligence != avg) variance += `INT:+${creature.intelligence} `;

            return variance;
        }

        const convertDamage = (creature, prevDamage) => {
            const damageConverted = Math.ceil(prevDamage / 2);
            // 5 is the approximate damage of a one-handed weapon and 3 is that of a natural weapon.  This is subtracted from the damage to undo the dice roll buff
            // Two-Handed is normally 7 but we use 5 because a TH weapon comes with downsides and is approximately balanced to a One Handed Weapon
            const plus5 = damageConverted >= 5 ? '+' : '';
            const plus3 = damageConverted >= 3 ? '+' : '';
            if (creature.dexterity > creature.strength) return `OH${plus5}${damageConverted - 5}`;
            else return damageConverted > 5 ? `TH+${damageConverted - 5}` : `NW${plus3}${damageConverted - 3}`;
        }

        const convertHP = (old) => {
            if (old < 10) return 1;
            else return Math.ceil(old / 20) + 1;
        }

        const convertArmorClass = (old) => {
            let ac = Math.ceil(old / 3) + 3;
            return ac > 10 ? 10 : ac;
        }

        if (is5e) {

            let creature = {
                strength: convertModifier(getValue(strKeywords, val)) || 0,
                intelligence: convertModifier(getValue(intKeywords, val)) || 0,
                dexterity: convertModifier(getValue(dexKeywords, val)) || 0,
                charisma: convertModifier(getValue(chaKeywords, val)) || 0
            }

            let avg = averageMod(creature) || 0;
            let vari = getVariance(avg, creature) || '';
            let damage = getValue(dmgKeywords, val) || 4;
            let damageString = convertDamage(creature, damage);
            let hp = getValue(hitpoints, val);
            let newHP = convertHP(hp) || 1;
            let fiveArmor = getValue(armorKeywords, val);
            let newFiveArmor = convertArmorClass(fiveArmor) || 8;
            let isRanged = hasValue("Ranged Weapon", val);

            return {
                creatureStats: `STATS:+${avg} / ${vari} Hearts: ${newHP}. Damage: ${isRanged ? 'R' : ''}${damageString}. Armor: ${newFiveArmor}.`,
                strength: creature.strength,
                intelligence: creature.intelligence,
                dexterity: creature.dexterity,
                charisma: creature.charisma,
                hearts: newHP,
                armor: newFiveArmor,
                type: damageString.slice(0, 2) || "NW",
                damage: Number(damageString.slice(2, damageString.length) || 0),
                ranged: isRanged // Ranged Weapon Attack
            };
        }
        else {
            let stats = Number(getValue(statKeywords, val) || 0);
            let th = getValue(thKeywords, val);
            let oh = getValue(ohKeywords, val);
            let nw = getValue(nwKeywords, val);
            let type = th !== undefined ? "TH" : oh !== undefined ? "OH" : "NW";
            let damage = type === "TH" ? th : type === "OH" ? oh : nw || 0;
            let ranged = getValue(rangedKeywords, val) === undefined ? false : true;

            let result = {
                strength: Number(getValue(strKeywords, val)) || stats || 0,
                dexterity: Number(getValue(dexKeywords, val)) || stats || 0,
                charisma: Number(getValue(chaKeywords, val)) || stats || 0,
                intelligence: Number(getValue(intKeywords, val)) || stats || 0,
                hearts: Number(getValue(heartKeywords, val)) || 1,
                armor: Number(getValue(armorKeywords, val)) || 8,
                type,
                ranged,
                damage : Number(damage)
            }

            let vari = getVariance(stats, {
                strength: result.strength,
                intelligence: result.intelligence,
                dexterity: result.dexterity,
                charisma: result.charisma
            }) || '';
            result.creatureStats = `STATS:+${stats} / ${vari} Hearts: ${result.hearts}. ${result.type}${result.ranged ? 'R' : ''}${result.damage}. Armor: ${result.armor}.`;

            return result;
        }
    }

    window.convertCreature = convert;

})();