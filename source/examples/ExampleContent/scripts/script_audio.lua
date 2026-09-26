local t = {
    sounds = {}
}

function setupGameSounds()
    precacheSoundClip("ui/click", true)
    precacheSoundClip("ui/back", true)
end

function precacheSoundClip(name, decompress)
    t.sounds[name] = audio:precacheSound(name, decompress)
end
