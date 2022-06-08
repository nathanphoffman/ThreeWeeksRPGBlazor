(() => {

    function extract(keyword, source) {
        if (!source) return '';

        let index = source.toUpperCase().indexOf(keyword.toUpperCase());
        if (index === -1) index = source.indexOf(keyword.toUpperCase());
        if (index === -1) return '';

        let result = source.slice(index + keyword.length, source.length - 1);
        result = result.replaceAll(' ', '');

        const firstNumber = result.match(/\d+/);
        if (!firstNumber) return '';
        const firstNumberIndex = result.indexOf(firstNumber[0]);
        if (firstNumberIndex > 3) return ''
        else if (firstNumberIndex > 0 && result[firstNumberIndex - 1] === '-') return '-' + firstNumber[0];
        else return firstNumber[0];

    }

    function newKeywords(sourceKeywords, char) {
        return sourceKeywords.map(x => x + char);
    }


    function getValue(keywords, source) {

        let postfixKeywords = [...newKeywords(keywords, ' '), ...newKeywords(keywords, ':'), ...newKeywords(keywords, '\n')];

        let results = postfixKeywords.map(keyword => extract(keyword, source));
        return results.find(value => !isNaN(value) && value != '');

    }

    function convert(val) {
        const strKeywords = ['str', 'strength'];
        const dexKeywords = ['dex', 'dexterity'];
        const intKeywords = ['int', 'intelligence'];
        const chaKeywords = ['cha', 'charisma'];
        const dmgKeywords = ['hit', 'damage', 'dmg'];
        const hitpoints = ['hitpoints', 'hit points', 'hp'];
        const armorKeywords = ['AC', 'Armor Class'];

        function convertModifier(mod) {
            let fmod = Math.ceil((Number(mod) - 10) / 2);
            fmod = Math.ceil(fmod / 3);
            return fmod < 0 ? 0 : fmod;
        }

        function averageMod(creature) {
            let match = null;
            let arr = [creature.strength, creature.intelligence, creature.dexterity, creature.charisma];
            arr.sort().reduce((prev, curr) => curr === prev ? match = curr : curr);
            return match !== null ? match : arr[0];
        }

        function getVariance(avg, creature) {
            let variance = '';
            if (creature.strength != avg) variance += `STR:+${creature.strength} `;
            if (creature.charisma != avg) variance += `CHA:+${creature.charisma} `;
            if (creature.dexterity != avg) variance += `DEX:+${creature.dexterity} `;
            if (creature.intelligence != avg) variance += `INT:+${creature.intelligence} `;

            return variance;
        }

        function convertDamage(creature, prevDamage) {

            const damageConverted = Math.ceil(prevDamage / 2);
            if (creature.dexterity > creature.strength) return `OH${damageConverted - 5}`;
            else return damageConverted > 5 ? `TH+${damageConverted - 5}` : `NH${damageConverted - 3}`;

        }

        function convertHP(old) {
            if (old < 10) return 1;
            else return Math.ceil(old / 20) + 1;
        }

        function convertArmorClass(old) {
            let ac = Math.ceil(old / 3) + 3;
            return ac > 10 ? 10 : ac;
        }



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

        return `STATS:+${avg} / ${vari} Hearts: ${newHP}.  Damage: ${damageString}.  Armor: ${newFiveArmor}`;
    }

    window.convertCreature = convert;

})();