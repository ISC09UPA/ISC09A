import { useCallback, useEffect, useState } from 'react';

import { cardsApi } from '../services/api';
import type { Card, PagedResponse } from '../types/api';
import type { LanguageCode } from '../types/language';
import { getErrorMessage } from '../utils/errorMessages';

type CardsResult = { status: 'error'; message: string } | { status: 'loaded'; data: PagedResponse<Card> };

export type CardsState = { status: 'loading' } | (CardsResult & { refreshing: boolean });

/** First page of the user's cards that have a translation in `language`. */
export function useCards(language: LanguageCode) {
  const [reloadCount, setReloadCount] = useState(0);
  const [refreshing, setRefreshing] = useState(false);
  // The result remembers which request produced it; a result for another request means "loading".
  const [result, setResult] = useState<{ requestKey: string; value: CardsResult } | null>(null);
  const requestKey = `${language}#${reloadCount}`;

  useEffect(() => {
    let cancelled = false;
    const settle = (value: CardsResult) => {
      if (!cancelled) {
        setResult({ requestKey, value });
        setRefreshing(false);
      }
    };

    cardsApi.getCards({ language }).then(
      (data) => settle({ status: 'loaded', data }),
      (error: unknown) => settle({ status: 'error', message: getErrorMessage(error) }),
    );

    return () => {
      cancelled = true;
    };
  }, [language, requestKey]);

  const reload = useCallback(() => setReloadCount((count) => count + 1), []);
  const refresh = useCallback(() => {
    setRefreshing(true);
    setReloadCount((count) => count + 1);
  }, []);

  // While refreshing, keep showing the previous list instead of a full-screen spinner.
  const current = result && (result.requestKey === requestKey || refreshing) ? result.value : null;
  const state: CardsState = current ? { ...current, refreshing } : { status: 'loading' };

  return { state, reload, refresh };
}
