<script setup lang="ts">
import { Sparkles } from '@lucide/vue'
import BaseCard from '../ui/BaseCard.vue'

const selectedProvider = defineModel<string>({ required: true })
</script>

<template>
  <BaseCard class="space-y-4">
    <div class="flex items-center gap-2 border-b border-[#e4e2d8] pb-3">
      <Sparkles class="w-4.5 h-4.5 text-slate-700" />
      <h3 class="font-bold text-[#1c1b17] text-xs uppercase tracking-wider">Model Selector</h3>
    </div>
    <p class="text-[10px] text-slate-500">Choose which AI brain to route the contextual query through.</p>
    
    <div class="space-y-2">
      <!-- Claude -->
      <label 
        class="flex items-center justify-between p-3 rounded-xl border cursor-pointer transition-all duration-200"
        :class="selectedProvider === 'Claude' 
          ? 'bg-black/[0.02] border-[#1c1b17] text-[#1c1b17]' 
          : 'border-[#e4e2d8] hover:border-slate-400 text-slate-400'"
      >
        <div class="flex items-center gap-2">
          <input 
            type="radio" 
            value="Claude" 
            v-model="selectedProvider" 
            class="hidden" 
          />
          <span class="w-2.5 h-2.5 rounded-full" :class="selectedProvider === 'Claude' ? 'bg-[#1c1b17]' : 'bg-slate-300'"></span>
          <span class="text-xs font-bold">Claude 3.5 Sonnet</span>
        </div>
        <span class="text-[9px] uppercase font-bold tracking-wider opacity-60">HQ AI</span>
      </label>

      <!-- GPT -->
      <label 
        class="flex items-center justify-between p-3 rounded-xl border cursor-pointer transition-all duration-200"
        :class="selectedProvider === 'GPT' 
          ? 'bg-black/[0.02] border-[#1c1b17] text-[#1c1b17]' 
          : 'border-[#e4e2d8] hover:border-slate-400 text-slate-400'"
      >
        <div class="flex items-center gap-2">
          <input 
            type="radio" 
            value="GPT" 
            v-model="selectedProvider" 
            class="hidden" 
          />
          <span class="w-2.5 h-2.5 rounded-full" :class="selectedProvider === 'GPT' ? 'bg-[#1c1b17]' : 'bg-slate-300'"></span>
          <span class="text-xs font-bold">GPT-4o Mini</span>
        </div>
        <span class="text-[9px] uppercase font-bold tracking-wider opacity-60">Balanced</span>
      </label>

      <!-- Gemini -->
      <label 
        class="flex items-center justify-between p-3 rounded-xl border cursor-pointer transition-all duration-200"
        :class="selectedProvider === 'Gemini' 
          ? 'bg-black/[0.02] border-[#1c1b17] text-[#1c1b17]' 
          : 'border-[#e4e2d8] hover:border-slate-400 text-slate-400'"
      >
        <div class="flex items-center gap-2">
          <input 
            type="radio" 
            value="Gemini" 
            v-model="selectedProvider" 
            class="hidden" 
          />
          <span class="w-2.5 h-2.5 rounded-full" :class="selectedProvider === 'Gemini' ? 'bg-[#1c1b17]' : 'bg-slate-300'"></span>
          <span class="text-xs font-bold">Gemini 1.5 Pro</span>
        </div>
        <span class="text-[9px] uppercase font-bold tracking-wider opacity-60">Creative</span>
      </label>

      <!-- MiniMax -->
      <label 
        class="flex items-center justify-between p-3 rounded-xl border cursor-pointer transition-all duration-200"
        :class="selectedProvider === 'MiniMax' 
          ? 'bg-black/[0.02] border-[#1c1b17] text-[#1c1b17]' 
          : 'border-[#e4e2d8] hover:border-slate-400 text-slate-400'"
      >
        <div class="flex items-center gap-2">
          <input 
            type="radio" 
            value="MiniMax" 
            v-model="selectedProvider" 
            class="hidden" 
          />
          <span class="w-2.5 h-2.5 rounded-full" :class="selectedProvider === 'MiniMax' ? 'bg-[#1c1b17]' : 'bg-slate-300'"></span>
          <span class="text-xs font-bold">MiniMax M2.5</span>
        </div>
        <span class="text-[9px] uppercase font-bold tracking-wider opacity-60">Fast & Smart</span>
      </label>
    </div>
  </BaseCard>
</template>
