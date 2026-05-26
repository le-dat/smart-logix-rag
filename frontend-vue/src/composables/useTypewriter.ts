import { ref } from 'vue'

export interface TypewriterOptions {
  baseSpeed?: number
  fastSpeed?: number
}

export function useTypewriter(options: TypewriterOptions = {}) {
  const isTyping = ref(false)
  const baseSpeed = options.baseSpeed ?? 15
  const fastSpeed = options.fastSpeed ?? 8

  const type = (
    text: string,
    onProgress: (char: string, accumulated: string) => void,
    onComplete?: () => void
  ) => {
    isTyping.value = true
    let accumulated = ''
    let index = 0
    // Dynamic typing speed based on content length
    const speed = text.length > 500 ? fastSpeed : baseSpeed

    const tick = () => {
      if (index < text.length) {
        const char = text.charAt(index)
        accumulated += char
        onProgress(char, accumulated)
        index++
        setTimeout(tick, speed)
      } else {
        isTyping.value = false
        if (onComplete) onComplete()
      }
    }

    tick()
  }

  return {
    isTyping,
    type
  }
}
