import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import type { ApiError } from "@/types/apiError.types";
import type { UserStatus } from "@/types/auth.types";
import { accessKeys } from "../queries/useMembers";
import type { Member, MemberCounts, MemberList } from "../../types/access.types";

export function countMembers(items: Member[]): MemberCounts {
  const n = (s: UserStatus) => items.filter((m) => m.status === s).length;
  return { all: items.length, pending: n("Pending"), approved: n("Approved"), rejected: n("Rejected"), suspended: n("Suspended") };
}

/** Optimistic status flip on the members list: row updates now, rolls back with a toast on failure. */
export function useOptimisticMemberMutation(
  mutationFn: (id: string) => Promise<Member>,
  status: UserStatus,
  errorMessage: string,
) {
  const queryClient = useQueryClient();

  return useMutation<Member, ApiError, string, { previous?: MemberList }>({
    mutationFn,
    onMutate: async (id) => {
      await queryClient.cancelQueries({ queryKey: accessKeys.members() });
      const previous = queryClient.getQueryData<MemberList>(accessKeys.members());
      if (previous) {
        const items = previous.items.map((m) => (m.id === id ? { ...m, status } : m));
        queryClient.setQueryData<MemberList>(accessKeys.members(), { items, counts: countMembers(items) });
      }
      return { previous };
    },
    onError: (error, _id, context) => {
      if (context?.previous) queryClient.setQueryData(accessKeys.members(), context.previous);
      toast.error(error.message ?? errorMessage);
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: accessKeys.all }),
  });
}
